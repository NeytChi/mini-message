using Common.Logging;
using System.Collections.Generic;

namespace Common.Routing
{
    public delegate void RequestAction(ref HttpRequest request);

    public static class Router
    {
        public static Dictionary<string,Route> routes = new Dictionary<string, Route>();

        public static void AddRoute(Route route)
        {
            route.route_url = route.route_url.ToLower();
            route.route_url = "v1.0/" + route.route_url;
            routes.Add(route.route_url, route);
            Logger.WriteLog("Add route->" + route.route_url + ".", LogLevel.Usual);
        }
        public static Route GetRoute(ref string route_url)
        {
            if (routes.ContainsKey(route_url))
            {
                Logger.WriteLog("Define action route.", LogLevel.Usual);
                return routes[route_url];
            }
            else
            {
                Logger.WriteLog("Can't find action by route.", LogLevel.Warning);
                return null;
            }
        }
    }
    public class Route
    {
        public string route_method;
        public string route_url;
        public RequestAction action;

        public Route(string route_method, string route_url, RequestAction action)
        {
            this.route_method = route_method;
            this.route_url = route_url;
            this.action = action;
        }
    }
}

























