
using System.Collections.Concurrent;

namespace SistemaSubsidios_CASATIC.Services
{
    public class OtpService
    {
        private readonly ConcurrentDictionary<string, (string Codigo, DateTime Expira, DateTime UltimoEnvio)> _otps  = new();
        
        private const int cooldown = 30;
        public (bool Permitido, string? Codigo, int SegundosRestantes) Generar(string correo)
        {
            if (_otps.TryGetValue(correo, out var data))
            {
                var segundos = (int)(data.UltimoEnvio.AddSeconds(cooldown) - DateTime.Now).TotalSeconds;

                if (segundos > 0)
                {
                    return (false, null, segundos);
                }
            }

            var random = new Random();
            var codigo = random.Next(100000, 999999).ToString();

            _otps[correo] = (codigo, DateTime.Now.AddMinutes(5), DateTime.Now);

            return (true, codigo, 0);
        }


        public bool Validar(string correo, string ingreso)
        {
            if (!_otps.TryGetValue(correo, out var data))
                return false;

            if (DateTime.Now > data.Expira)
            {
                _otps.TryRemove(correo, out _);
                return false;
            }

            bool valido = data.Codigo == ingreso;

            if (valido)
                _otps.TryRemove(correo, out _);

            return valido;
        }
    }

    

}
