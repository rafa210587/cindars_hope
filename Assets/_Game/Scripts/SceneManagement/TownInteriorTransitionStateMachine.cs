using System;
using CindarsHope.Foundation;

namespace CindarsHope.SceneManagement
{
    /// <summary>Pure single-operation transaction. Side effects belong to the visit controller.</summary>
    public sealed class TownInteriorTransitionStateMachine
    {
        private string _operationId = string.Empty;

        public TownVisitPhase Phase { get; private set; } = TownVisitPhase.Idle;
        public string OperationId => _operationId;
        public string FailureReason { get; private set; } = string.Empty;
        public bool IsTransitioning => Phase != TownVisitPhase.Idle;

        public bool TryBegin(TownInteriorVisitRequest request, out string reason)
        {
            if (Phase != TownVisitPhase.Idle)
            {
                reason = "Another interior visit is in progress.";
                return false;
            }
            if (!request.IsValid)
            {
                reason = "Interior visit requires stable operation, house and anchor IDs.";
                return false;
            }
            _operationId = request.OperationId;
            FailureReason = string.Empty;
            Phase = TownVisitPhase.Opening;
            reason = string.Empty;
            return true;
        }

        public bool Advance(string operationId, TownVisitPhase expected, TownVisitPhase next)
        {
            if (!Matches(operationId) || Phase != expected || !IsLegalForwardEdge(expected, next))
                return false;
            Phase = next;
            return true;
        }

        public void Fail(string operationId, string reason) => TryFail(operationId, reason);

        public bool TryFail(string operationId, string reason)
        {
            if (!Matches(operationId) || Phase == TownVisitPhase.Idle || Phase == TownVisitPhase.RollingBack)
                return false;
            FailureReason = string.IsNullOrWhiteSpace(reason) ? "Interior visit failed." : reason;
            Phase = TownVisitPhase.RollingBack;
            return true;
        }

        public bool TryTimeout(string operationId, float elapsedSeconds, float timeoutSeconds)
        {
            if (float.IsNaN(elapsedSeconds) || float.IsInfinity(elapsedSeconds) || elapsedSeconds < 0f ||
                float.IsNaN(timeoutSeconds) || float.IsInfinity(timeoutSeconds) || timeoutSeconds <= 0f ||
                elapsedSeconds < timeoutSeconds)
                return false;
            return TryFail(operationId, "Interior visit timed out.");
        }

        public void Complete(string operationId)
        {
            if (!Matches(operationId) || Phase != TownVisitPhase.FadingIn) return;
            Reset();
        }

        public bool CompleteRollback(string operationId)
        {
            if (!Matches(operationId) || Phase != TownVisitPhase.RollingBack) return false;
            Reset();
            return true;
        }

        private bool Matches(string operationId) =>
            !string.IsNullOrEmpty(operationId) && string.Equals(_operationId, operationId, StringComparison.Ordinal);

        private static bool IsLegalForwardEdge(TownVisitPhase from, TownVisitPhase to)
        {
            switch (from)
            {
                case TownVisitPhase.Opening: return to == TownVisitPhase.Armed;
                case TownVisitPhase.Armed: return to == TownVisitPhase.FadingOut;
                case TownVisitPhase.FadingOut: return to == TownVisitPhase.Loading;
                case TownVisitPhase.Loading: return to == TownVisitPhase.Placing;
                case TownVisitPhase.Placing: return to == TownVisitPhase.FadingIn;
                default: return false;
            }
        }

        private void Reset()
        {
            Phase = TownVisitPhase.Idle;
            _operationId = string.Empty;
            FailureReason = string.Empty;
        }
    }
}
