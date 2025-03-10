using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class AuthAttribute : ActionFilterAttribute
{
  public override void OnActionExecuting(ActionExecutingContext context)
  {
    var userId = context.HttpContext.Session.GetInt32("UserId");

    string controller = context.RouteData.Values["controller"]?.ToString() ?? "";
    string action = context.RouteData.Values["action"]?.ToString() ?? "";


    if (controller.Equals("Auth", StringComparison.OrdinalIgnoreCase) &&
        (action.Equals("Login", StringComparison.OrdinalIgnoreCase) ||
         action.Equals("Logout", StringComparison.OrdinalIgnoreCase)))
    {
      base.OnActionExecuting(context);
      return;
    }
    if (userId == null)
    {
      context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
      {
        controller = "Auth",
        action = "Login"
      }));
    }

    base.OnActionExecuting(context);
  }
}
