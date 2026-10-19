namespace CindarsHope.UI.Runtime
{
    /// <summary>
    /// fable_38 — ícones pintados sobre as células do minimapa. Cada ícone tem uma cor placeholder
    /// no <see cref="MinimapRenderer"/> (v1: shapes/cores). O minimapa NUNCA mostra inimigos
    /// (decisão da direction: leitura de combate é do palco) — não há entrada de inimigo aqui.
    /// </summary>
    public enum MinimapIconKind
    {
        Player = 0,            // branco — sempre presente
        Npc = 1,               // amarelo — Town/Farm
        Board = 2,             // azul — mural/board (Town/Farm)
        CaveEntrance = 3,      // vermelho — entrada da caverna (Town/Farm)
        StairsDown = 4,        // saída/descida (Cave, quando célula revelada)
        StairsUp = 5,          // entrada/subida (Cave, quando célula revelada)
        WanderingMerchant = 6  // mercador errante (Cave, quando spawnado e revelado)
    }
}
