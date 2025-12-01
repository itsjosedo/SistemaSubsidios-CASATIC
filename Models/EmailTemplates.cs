namespace SistemaSubsidios_CASATIC.Services
{
    public static class EmailTemplates
    {
        public static string OtpTemplate(string codigo)
        {
            return $@"
            <div style='font-family: Arial, sans-serif; padding: 20px;'>
                
                <h2 style='color: #0033A0;'>Sistema Subsidios</h2>

                <p>Tu código de verificación es:</p>

                <div style='
                    background:#0033A0;
                    color:white;
                    padding: 15px;
                    text-align:center;
                    font-size: 32px;
                    border-radius: 8px;
                    width: 200px;
                    margin: auto;
                '>
                    {codigo}
                </div>

                <p style='margin-top:20px;'>
                    Este código expira en <strong>5 minutos</strong>.
                </p>

                <p style='margin-top:30px; font-size: 12px; color: #666;'>
                    Si no fuiste tú quien solicitó este código, ignora este mensaje.
                </p>
            </div>";
        }
    }
}
