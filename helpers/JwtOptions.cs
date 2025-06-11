namespace MentalHealth_BackEnd.helpers
{
    public class JwtOptions
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int LifeTime { get; set; }
        public string SecretKey { get; set; }
    }
}
