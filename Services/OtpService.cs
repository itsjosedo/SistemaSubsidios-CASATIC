
using System.Collections.Concurrent;

namespace SistemaSubsidios_CASATIC.Services
{
    public class OtpService
    {
        private readonly ConcurrentDictionary<string, (string Codigo, DateTime Expira)> _otps
            = new();

        public string Generar(string correo)
        {
            var random = new Random();
            var codigo = random.Next(100000, 999999).ToString(); // 6 dígitos

            _otps[correo] = (codigo, DateTime.Now.AddMinutes(5));

            return codigo;
        }

        public bool Validar(string correo, string ingreso)
        {
            if (!_otps.ContainsKey(correo))
                return false;

            var (codigoGuardado, expira) = _otps[correo];

            if (DateTime.Now > expira)
            {
                _otps.TryRemove(correo, out _);
                return false;
            }

            bool esValido = codigoGuardado == ingreso;

            if (esValido)
                _otps.TryRemove(correo, out _);

            return esValido;
        }
    }

    

}
