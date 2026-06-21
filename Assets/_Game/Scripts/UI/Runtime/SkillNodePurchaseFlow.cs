namespace CindarsHope.UI.Runtime
{
    /// <summary>
    /// fable_14 CA-2 canonical rule: a skill point can NEVER be spent without the player first
    /// seeing the node detail and then confirming. This pure-C# state machine enforces the
    /// "detail before purchase + confirmation" gate so the rule is EditMode-testable rather
    /// than buried in the Canvas view.
    ///
    /// Flow per focused node: Browsing -> (select) Detail -> (confirm) Confirm -> (confirm) Purchase.
    /// A purchase request is only emitted from the Confirm state. Esc/cancel from any state
    /// steps back toward Browsing. Equipment compare uses an analogous never-act-on-hover rule
    /// handled in its own view; this type is skill-tree specific.
    /// </summary>
    public sealed class SkillNodePurchaseFlow
    {
        public enum Stage
        {
            Browsing,   // navigating nodes, nothing selected
            Detail,     // node detail drawer shown (mandatory before purchase)
            Confirm     // explicit "spend 1 skill point?" confirmation shown
        }

        public Stage CurrentStage { get; private set; } = Stage.Browsing;

        public string SelectedNodeId { get; private set; }

        /// <summary>Select a node — always opens the detail drawer first (never purchases).</summary>
        public void SelectNode(string nodeId)
        {
            SelectedNodeId = nodeId;
            CurrentStage = string.IsNullOrEmpty(nodeId) ? Stage.Browsing : Stage.Detail;
        }

        /// <summary>
        /// Advance the flow on a confirm input. Returns true ONLY on the transition that
        /// actually authorizes the purchase (Confirm -> purchase). At Detail it opens the
        /// confirmation; at Browsing it does nothing (no node selected).
        /// </summary>
        public bool Confirm()
        {
            switch (CurrentStage)
            {
                case Stage.Detail:
                    CurrentStage = Stage.Confirm;
                    return false;
                case Stage.Confirm:
                    // Purchase authorized. Reset back to browsing for the next node.
                    CurrentStage = Stage.Browsing;
                    var purchased = !string.IsNullOrEmpty(SelectedNodeId);
                    return purchased;
                default:
                    return false;
            }
        }

        /// <summary>Cancel/Esc steps one stage back toward Browsing.</summary>
        public void Cancel()
        {
            CurrentStage = CurrentStage switch
            {
                Stage.Confirm => Stage.Detail,
                Stage.Detail => Stage.Browsing,
                _ => Stage.Browsing
            };
        }

        /// <summary>True when a purchase may be authorized by the next Confirm.</summary>
        public bool CanPurchaseNow => CurrentStage == Stage.Confirm && !string.IsNullOrEmpty(SelectedNodeId);

        public void Reset()
        {
            CurrentStage = Stage.Browsing;
            SelectedNodeId = null;
        }
    }
}
