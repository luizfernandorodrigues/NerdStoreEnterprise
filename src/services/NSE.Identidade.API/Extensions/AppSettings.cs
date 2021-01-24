namespace NSE.Identidade.API.Extensions
{
    public class AppSettings
    {
        #region Propriedades

        public string Secret { get; set; }
        public int ExpiracaoHoras { get; set; }
        public string Emissor { get; set; }
        public string ValidoEm { get; set; }

        #endregion
    }
}
