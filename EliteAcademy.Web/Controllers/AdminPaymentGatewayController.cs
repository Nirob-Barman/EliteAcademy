using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EliteAcademy.Web.Controllers
{
    [NonController]
    [Authorize(Roles = "Admin")]
    public class AdminPaymentGatewayRouteStub
    {
    }
}
