using Microsoft.AspNetCore.Authorization;
using NSE.WebAPI.Core.Controllers;

namespace NSE.Pedidos.API.Controllers
{
    [Authorize]
    public class VoucherController : MainController
    {
    }
}
