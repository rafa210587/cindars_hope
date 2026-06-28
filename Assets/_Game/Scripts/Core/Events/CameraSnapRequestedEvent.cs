namespace CindarsHope.Core.Events
{
    /// <summary>
    /// Pede à câmera para CORTAR (snap) imediatamente para o alvo, em vez de fazer o
    /// SmoothDamp habitual. Publicado quando o jogador é teleportado dentro da mesma cena
    /// (porta de casa/interior): sem o snap, a câmera "voaria" suavemente pelo mapa inteiro
    /// até o interior (y &gt; +40), dando a sensação de "ir para um lugar distante e voltar".
    ///
    /// Mantém o GameEventBus como único canal de comunicação — o emissor (DoorInteractable)
    /// não precisa de referência direta à câmera nem de busca global de cena.
    /// </summary>
    public readonly struct CameraSnapRequestedEvent
    {
    }
}
