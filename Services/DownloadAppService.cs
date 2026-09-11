using PotyIaApi.Interfaces;

namespace PotyIaApi.Services
{
    public class DownloadAppService
    {
        private readonly IDownloadAppRepositorio _downloadAppRepositorio;

        private const string LinkAndroid =
            "https://play.google.com/store/apps/details?id=br.com.bebidaspoty.potyia";

        private const string LinkPotyIaWeb =
            "https://SEU-LINK-DO-POTYIA-WEB";

        public DownloadAppService(
            IDownloadAppRepositorio downloadAppRepositorio)
        {
            _downloadAppRepositorio = downloadAppRepositorio;
        }

        public DownloadAppResultado DownloadApp(string userAgent)
        {
            // ANDROID
            if (!string.IsNullOrWhiteSpace(userAgent) &&
                userAgent.Contains(
                    "Android",
                    StringComparison.OrdinalIgnoreCase))
            {
                return new DownloadAppResultado
                {
                    Status = DownloadAppStatus.Sucesso,
                    Link = LinkAndroid
                };
            }

            // IOS
            if (!string.IsNullOrWhiteSpace(userAgent) &&
                EhDispositivoApple(userAgent))
            {
                var linkApple = _downloadAppRepositorio.DownloadAppApple();

                if (string.IsNullOrWhiteSpace(linkApple))
                {
                    return new DownloadAppResultado
                    {
                        Status = DownloadAppStatus.SemLinksDisponiveis
                    };
                }

                return new DownloadAppResultado
                {
                    Status = DownloadAppStatus.Sucesso,
                    Link = linkApple
                };
            }

            // PC, MAC, LINUX OU QUALQUER OUTRO DISPOSITIVO
            return new DownloadAppResultado
            {
                Status = DownloadAppStatus.Sucesso,
                Link = LinkPotyIaWeb
            };
        }

        private bool EhDispositivoApple(string userAgent)
        {
            return userAgent.Contains(
                       "iPhone",
                       StringComparison.OrdinalIgnoreCase)
                   ||
                   userAgent.Contains(
                       "iPad",
                       StringComparison.OrdinalIgnoreCase)
                   ||
                   userAgent.Contains(
                       "iPod",
                       StringComparison.OrdinalIgnoreCase);
        }
    }

    public class DownloadAppResultado
    {
        public DownloadAppStatus Status { get; set; }

        public string? Link { get; set; }
    }

    public enum DownloadAppStatus
    {
        Sucesso,
        SemLinksDisponiveis
    }
}