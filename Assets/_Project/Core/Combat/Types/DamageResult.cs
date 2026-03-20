namespace Wordania.Core.Combat
{
    public struct DamageResult
    {
        public DamagePayload Payload;
        public float FinalDamage;
        public bool WasEvaded;

        public DamageResult(DamagePayload payload, float finalDamage, bool wasEvaded)
        {
           Payload = payload;
           FinalDamage = finalDamage;
           WasEvaded = wasEvaded;
        } 
    }
}