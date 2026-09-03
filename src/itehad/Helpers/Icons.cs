using System;
using System.Web;
using System.Web.Mvc;

namespace itehad.Helpers
{
    public static class Icons
    {
        private static IHtmlString Svg(string inner, string cssClass = "icon")
        {
            var svg = "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\" stroke-linejoin=\"round\" class=\"" + cssClass + "\" aria-hidden=\"true\">" + inner + "</svg>";
            return new MvcHtmlString(svg);
        }

        public static IHtmlString Dashboard(string cssClass = "icon") => Svg(
            "<rect x=\"3\" y=\"3\" width=\"7\" height=\"7\" rx=\"1.5\"/><rect x=\"14\" y=\"3\" width=\"7\" height=\"7\" rx=\"1.5\"/><rect x=\"3\" y=\"14\" width=\"7\" height=\"7\" rx=\"1.5\"/><rect x=\"14\" y=\"14\" width=\"7\" height=\"7\" rx=\"1.5\"/>", cssClass);

        public static IHtmlString Car(string cssClass = "icon") => Svg(
            "<path d=\"M3 13l1.5-4.5A2 2 0 0 1 6.4 7h11.2a2 2 0 0 1 1.9 1.5L21 13\"/><rect x=\"2.5\" y=\"13\" width=\"19\" height=\"5\" rx=\"1.5\"/><circle cx=\"7\" cy=\"18.5\" r=\"1.5\"/><circle cx=\"17\" cy=\"18.5\" r=\"1.5\"/>", cssClass);

        public static IHtmlString List(string cssClass = "icon") => Svg(
            "<line x1=\"8\" y1=\"6\" x2=\"21\" y2=\"6\"/><line x1=\"8\" y1=\"12\" x2=\"21\" y2=\"12\"/><line x1=\"8\" y1=\"18\" x2=\"21\" y2=\"18\"/><line x1=\"3\" y1=\"6\" x2=\"3.01\" y2=\"6\"/><line x1=\"3\" y1=\"12\" x2=\"3.01\" y2=\"12\"/><line x1=\"3\" y1=\"18\" x2=\"3.01\" y2=\"18\"/>", cssClass);

        public static IHtmlString Users(string cssClass = "icon") => Svg(
            "<circle cx=\"9\" cy=\"8\" r=\"3.2\"/><path d=\"M3 20c0-3.3 2.7-6 6-6s6 2.7 6 6\"/><circle cx=\"17\" cy=\"9\" r=\"2.6\"/><path d=\"M15.5 14.2c2.6.3 4.5 2.6 4.5 5.3\"/>", cssClass);

        public static IHtmlString IdCard(string cssClass = "icon") => Svg(
            "<rect x=\"2.5\" y=\"5\" width=\"19\" height=\"14\" rx=\"2\"/><circle cx=\"8\" cy=\"12\" r=\"2.2\"/><path d=\"M5 16.5c0-1.8 1.4-3 3-3s3 1.2 3 3\"/><line x1=\"13.5\" y1=\"9\" x2=\"18.5\" y2=\"9\"/><line x1=\"13.5\" y1=\"13\" x2=\"18.5\" y2=\"13\"/>", cssClass);

        public static IHtmlString Clock(string cssClass = "icon") => Svg(
            "<circle cx=\"12\" cy=\"12\" r=\"9\"/><polyline points=\"12,7 12,12 16,14\"/>", cssClass);

        public static IHtmlString BarChart(string cssClass = "icon") => Svg(
            "<line x1=\"4\" y1=\"20\" x2=\"20\" y2=\"20\"/><rect x=\"6\" y=\"12\" width=\"3\" height=\"8\"/><rect x=\"11\" y=\"7\" width=\"3\" height=\"13\"/><rect x=\"16\" y=\"3\" width=\"3\" height=\"17\"/>", cssClass);

        public static IHtmlString MapPin(string cssClass = "icon") => Svg(
            "<path d=\"M12 21s7-6.5 7-11.5A7 7 0 0 0 5 9.5C5 14.5 12 21 12 21z\"/><circle cx=\"12\" cy=\"9.5\" r=\"2.3\"/>", cssClass);

        public static IHtmlString Tag(string cssClass = "icon") => Svg(
            "<path d=\"M3 11.5V5a2 2 0 0 1 2-2h6.5a2 2 0 0 1 1.4.6l8 8a2 2 0 0 1 0 2.8l-6.5 6.5a2 2 0 0 1-2.8 0l-8-8A2 2 0 0 1 3 11.5z\"/><circle cx=\"7.5\" cy=\"7.5\" r=\"1.3\"/>", cssClass);

        public static IHtmlString PlusCircle(string cssClass = "icon") => Svg(
            "<circle cx=\"12\" cy=\"12\" r=\"9\"/><line x1=\"12\" y1=\"8\" x2=\"12\" y2=\"16\"/><line x1=\"8\" y1=\"12\" x2=\"16\" y2=\"12\"/>", cssClass);

        public static IHtmlString Edit(string cssClass = "icon") => Svg(
            "<path d=\"M4 20h4l10.5-10.5a2.1 2.1 0 0 0-3-3L5 17v3z\"/><line x1=\"13\" y1=\"5.5\" x2=\"18.5\" y2=\"11\"/>", cssClass);

        public static IHtmlString CheckCircle(string cssClass = "icon") => Svg(
            "<circle cx=\"12\" cy=\"12\" r=\"9\"/><polyline points=\"8,12.5 11,15.5 16,9\"/>", cssClass);

        public static IHtmlString XCircle(string cssClass = "icon") => Svg(
            "<circle cx=\"12\" cy=\"12\" r=\"9\"/><line x1=\"9\" y1=\"9\" x2=\"15\" y2=\"15\"/><line x1=\"15\" y1=\"9\" x2=\"9\" y2=\"15\"/>", cssClass);

        public static IHtmlString Menu(string cssClass = "icon") => Svg(
            "<line x1=\"3\" y1=\"6\" x2=\"21\" y2=\"6\"/><line x1=\"3\" y1=\"12\" x2=\"21\" y2=\"12\"/><line x1=\"3\" y1=\"18\" x2=\"21\" y2=\"18\"/>", cssClass);

        public static IHtmlString Phone(string cssClass = "icon") => Svg(
            "<path d=\"M4.5 3.5h3.2l1.6 4.2-2 1.8a13.5 13.5 0 0 0 6.2 6.2l1.8-2 4.2 1.6v3.2a1.5 1.5 0 0 1-1.6 1.5A17 17 0 0 1 3 5.1 1.5 1.5 0 0 1 4.5 3.5z\"/>", cssClass);

        public static IHtmlString Money(string cssClass = "icon") => Svg(
            "<circle cx=\"12\" cy=\"12\" r=\"9\"/><line x1=\"12\" y1=\"6.5\" x2=\"12\" y2=\"17.5\"/><path d=\"M15 9.2c0-1.2-1.3-2.2-3-2.2s-3 .9-3 2.1 1.3 1.9 3 2.1 3 .9 3 2.1-1.3 2.1-3 2.1-3-1-3-2.2\"/>", cssClass);

        public static IHtmlString ArrowSwap(string cssClass = "icon") => Svg(
            "<line x1=\"4\" y1=\"8\" x2=\"20\" y2=\"8\"/><polyline points=\"15,3 20,8 15,13\"/><line x1=\"20\" y1=\"16\" x2=\"4\" y2=\"16\"/><polyline points=\"9,11 4,16 9,21\"/>", cssClass);

        public static IHtmlString Truck(string cssClass = "icon") => IdCard(cssClass);

        public static IHtmlString TaxiLogo(string cssClass = "brand-logo")
        {
            var svg = "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 48 48\" class=\"" + cssClass + "\" aria-hidden=\"true\">"
                + "<defs><linearGradient id=\"logoGrad\" x1=\"0\" y1=\"0\" x2=\"1\" y2=\"1\">"
                + "<stop offset=\"0%\" stop-color=\"#ffd166\"/><stop offset=\"100%\" stop-color=\"#e29500\"/>"
                + "</linearGradient></defs>"
                + "<rect x=\"1\" y=\"1\" width=\"46\" height=\"46\" rx=\"14\" fill=\"url(#logoGrad)\"/>"
                + "<g fill=\"#12192c\">"
                + "<rect x=\"6\" y=\"35\" width=\"6\" height=\"6\"/><rect x=\"18\" y=\"35\" width=\"6\" height=\"6\"/><rect x=\"30\" y=\"35\" width=\"6\" height=\"6\"/>"
                + "</g>"
                + "<g fill=\"#ffffff\" opacity=\"0.9\">"
                + "<rect x=\"12\" y=\"35\" width=\"6\" height=\"6\"/><rect x=\"24\" y=\"35\" width=\"6\" height=\"6\"/><rect x=\"36\" y=\"35\" width=\"6\" height=\"6\"/>"
                + "</g>"
                + "<path d=\"M10 24l2-7a3 3 0 0 1 2.8-2h14.4a3 3 0 0 1 2.8 2l2 7\" stroke=\"#12192c\" stroke-width=\"2.2\" fill=\"none\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/>"
                + "<rect x=\"7\" y=\"22\" width=\"34\" height=\"9\" rx=\"3\" fill=\"#12192c\"/>"
                + "<rect x=\"13\" y=\"16.5\" width=\"6\" height=\"5\" rx=\"1\" fill=\"#eaf2ff\" opacity=\"0.9\"/>"
                + "<rect x=\"29\" y=\"16.5\" width=\"6\" height=\"5\" rx=\"1\" fill=\"#eaf2ff\" opacity=\"0.9\"/>"
                + "<circle cx=\"15\" cy=\"31.5\" r=\"3\" fill=\"#ffd166\" stroke=\"#12192c\" stroke-width=\"1.5\"/>"
                + "<circle cx=\"33\" cy=\"31.5\" r=\"3\" fill=\"#ffd166\" stroke=\"#12192c\" stroke-width=\"1.5\"/>"
                + "</svg>";
            return new MvcHtmlString(svg);
        }

        public static string TaxiLogoFaviconDataUri()
        {
            var svg = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 48 48'>"
                + "<rect x='1' y='1' width='46' height='46' rx='14' fill='#e29500'/>"
                + "<g fill='#12192c'><rect x='6' y='35' width='6' height='6'/><rect x='18' y='35' width='6' height='6'/><rect x='30' y='35' width='6' height='6'/></g>"
                + "<g fill='#ffffff'><rect x='12' y='35' width='6' height='6'/><rect x='24' y='35' width='6' height='6'/><rect x='36' y='35' width='6' height='6'/></g>"
                + "<path d='M10 24l2-7a3 3 0 0 1 2.8-2h14.4a3 3 0 0 1 2.8 2l2 7' stroke='#12192c' stroke-width='2.2' fill='none' stroke-linecap='round' stroke-linejoin='round'/>"
                + "<rect x='7' y='22' width='34' height='9' rx='3' fill='#12192c'/>"
                + "<circle cx='15' cy='31.5' r='3' fill='#ffd166' stroke='#12192c' stroke-width='1.5'/>"
                + "<circle cx='33' cy='31.5' r='3' fill='#ffd166' stroke='#12192c' stroke-width='1.5'/>"
                + "</svg>";
            return "data:image/svg+xml," + Uri.EscapeDataString(svg);
        }
    }
}
