namespace AgroSolutions.Domain.Messages
{
    public class SensorMessage
    {
        public Guid TalhaoId { get; set; }
        public int Umidade { get; set; }
        public DateTime DataAfericao { get; set; }
        public double Temperatura { get; set; }
        public int IndiceUv { get; set; }
        public decimal VelocidadeVento { get; set; }
    }
}
