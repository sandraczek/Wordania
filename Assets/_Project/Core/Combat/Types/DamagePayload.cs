using UnityEngine;

namespace Wordania.Core.Combat
{
    /// <summary>
    /// Przenosi pełny kontekst zdarzenia o zadaniu obrażeń.
    /// Struktura jest niemutowalna (readonly), co gwarantuje bezpieczeństwo wątkowe i chroni przed błędami zmiany stanu.
    /// </summary>
    public readonly struct DamagePayload
    {
        public readonly float Amount;
        public readonly DamageType Type;
        
        // Kto lub co zadało obrażenia? Używamy GameObject, by mieć dostęp do Transform (potrzebne do odrzutu).
        // Może być null (np. przy obrażeniach od upadku, gdzie nie ma konkretnego sprawcy).
        public readonly GameObject Instigator; 
        
        // Punkt w przestrzeni, w którym nastąpiło trafienie (przydatne do Particle System i pływającego tekstu).
        public readonly Vector2 HitPoint;      
        
        // W grach typu Terraria odrzut to kluczowa mechanika. Przekazujemy go w ładunku.
        public readonly float KnockbackForce;  

        public DamagePayload(
            float amount, 
            DamageType type, 
            GameObject instigator, 
            Vector2 hitPoint, 
            float knockbackForce = 0f)
        {
            // Senior-level: Zawsze walidujemy dane wejściowe u źródła. 
            // Obrażenia nigdy nie mogą być ujemne (od tego jest metoda ApplyHealing).
            Amount = Mathf.Max(0f, amount); 
            
            Type = type;
            Instigator = instigator;
            HitPoint = hitPoint;
            KnockbackForce = knockbackForce;
        }
    }
}