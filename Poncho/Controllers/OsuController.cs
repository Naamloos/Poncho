using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Poncho.Enums;
using Poncho.PacketHandling;
using System.Net;
using System.Reflection;

namespace Poncho.Controllers
{
    [ApiController]
    [Route("/")]
    public class OsuController : ControllerBase
    {
        private readonly ILogger<OsuController> _logger;

        public OsuController(ILogger<OsuController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public ContentResult Get()
        {
            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
                Content = @"
<!DOCTYPE HTML>
<html>
<head>
<title>poncho</title>
</head>
<body>
<pre>
          _-'-_
         /_-_-_\
 _______|-_-_-_-|________
(________________________)
! ! ! ! ! ! ! ! ! ! ! ! !
   P O N C H O - O S U

To connect to beef.moe's Poncho instance:

>  C:\Users\[YOUR NAME]\AppData\Local\osu!\osu!.exe -devserver beef.moe  <

The server does not have any 'real' communication right now, as it's still in development.
</pre>
</body>
</html>
            "
            };
        }

        [HttpGet("/menuicon")]
        public async Task MenuIcon()
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Poncho.MenuIcon.png");
            if (stream == null)
                return;

            this.Response.ContentType = "image/png";
            stream.Position = 0;
            stream.CopyTo(this.Response.Body);
        }

        [HttpGet("/wallpaper")]
        public async Task Wallpaper()
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Poncho.Wallpaper.jpg");
            if (stream == null)
                return;

            this.Response.ContentType = "image/jpeg";
            stream.Position = 0;
            stream.CopyTo(this.Response.Body);
        }

        [HttpGet("/{id}")]
        public async Task GetAsync([FromRoute]int id)
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Poncho.Avatar.png");
            if(stream == null)
                return;

            this.Response.ContentType = "image/png";
            stream.Position = 0;
            stream.CopyTo(this.Response.Body);
        }

        [HttpPost]
        public async Task PostAsync()
        {
            if (this.Request.Headers.UserAgent == "osu!")
            {
                this._logger.LogWarning("new osu connection.");
                // handle osu request.
                this.Response.Headers.Add("cho-protocol", "19"); // TODO find out meanings
                this.Response.Headers.ContentType = "application/octet-stream";

                Stream output = this.Response.Body;

                if (!this.Request.Headers.TryGetValue("osu-token", out StringValues token))
                {
                    this.Response.Headers.Add("cho-token", new Guid().ToString());
                    await new PacketSender(output)
                        .Protocol(19)
                        .LoginResult(69)
                        .Permissions(Permissions.Developer)
                        .MenuIcon($"https://{this.Request.Host.Host}/menuicon", "https://naamloos.dev")
                        .Presence(1, "PonchoBot", Permissions.Moderator, 0)
                        .Presence(69, "a", Permissions.Owner, 1)
                        .TestStats(Status.Unknown, "issa me ponchobot hahahehehe", 1)
                        .TestStats(Status.Idle, "", 69)
                        .JoinChannel("#osu")
                        .JoinChannel("#hentai")
                        .JoinChannel("#memes")
                        .JoinChannel("#venting")
                        .JoinChannel("#cringe")
                        .OpenChannel("#osu", "The main chat", 696969)
                        .ChannelInfo("#osu", "The main chat", 696969)
                        .ChannelInfo("#hentai", "Big anime boobies", 696969)
                        .ChannelInfo("#memes", "funni hahas", 696969)
                        .ChannelInfo("#venting", "Your problems go here", 696969)
                        .ChannelInfo("#cringe", "Home sweet home", 696969)
                        .ChannelInfoEnd()
                        .SendFriends(new int[] { 1 })
                        .SendChat("PonchoBot", "Welcome to Poncho, @a !", "#osu", 1)
                        .Notification("Welcome to Poncho, [username here]!")
                        .FlushAsync();
                    return;
                }
                this.Response.Headers.Add("cho-token", token.ToString());
            }
        }

        [HttpPost("/web/bancho_connect.php")]
        [HttpGet("/web/bancho_connect.php")]
        public async Task<string> GetBanchoConnect()
        {
            return "us";
        }

        [HttpPost("/web/osu-checktweets.php")]
        [HttpGet("/web/osu-checktweets.php")]
        public async Task<string> GetOsuTweets()
        {
            return "";
        }

        [HttpPost("/web/osu-getseasonal.php")]
        [HttpGet("/web/osu-getseasonal.php")]
        public async Task<IActionResult> GetSeasonalBackgrounds()
        {
            this.Response.ContentType = "application/json";
            return Content($"[\"https:\\/\\/{this.Request.Host.Host}\\/wallpaper\"]", "application/json");
        }
    }
}
