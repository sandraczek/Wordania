namespace Wordania.Core.Combat
{
    /// <summary>
    /// Definiuje rodzaj zadawanych obrażeń. Używane przez systemy zdrowia i pancerza 
    /// do kalkulacji redukcji obrażeń (mitigacji).
    /// </summary>
    public enum DamageType
    {
        Physical,       // Zwykłe ataki wręcz i dystansowe (miecze, łuki)
        Magical,        // Ataki magiczne (różdżki) - mogą ignorować część fizycznego pancerza
        Environmental,  // Środowisko (kolce, lawa)
        FallDamage,     // Upadek z wysokości
        TrueDamage      // Czyste obrażenia - ignorują WSZYSTKIE pancerze i odporności (np. utonięcie, wyjście poza mapę)
    }
}