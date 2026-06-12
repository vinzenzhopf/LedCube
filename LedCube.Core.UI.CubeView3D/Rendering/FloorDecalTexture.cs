using System.Numerics;
using SkiaSharp;

namespace LedCube.Core.UI.CubeView3D.Rendering;

/// <summary>
/// Renders the flat floor annotations (a "FRONT" arrow + label, and an arrow at the first LED
/// pointing along the first row / +X) into an RGBA texture via SkiaSharp. The texture is mapped
/// onto a horizontal quad covering the cube footprint, so all annotations lie flat on the base.
///
/// World→texture mapping (matches the quad UVs built in <see cref="ScenePropsRenderer"/>):
///   u = (x + E) / (2E)   (so +X = right)
///   v = (z + E) / (2E)   (so +Z = down; front = -Z = top of the image)
/// where E is the decal half-extent in world units.
/// </summary>
public static class FloorDecalTexture
{
    public readonly record struct Decal(byte[] Rgba, int Size);

    /// <param name="led0Floor">World (X,Z) of LED 0 on the base plane (depends on orientation).</param>
    /// <param name="firstAxisDir">World (X,Z) direction the first index advances; zero if it runs vertically.</param>
    public static Decal Create(int pixels, float halfExtentWorld, float footprintHalfWorld,
        Vector2 led0Floor, Vector2 firstAxisDir, bool includeLed0Arrow = true)
    {
        var s = pixels;
        var e = halfExtentWorld;
        var fh = footprintHalfWorld;

        float ToPx(float world) => (world + e) / (2f * e) * s;
        SKPoint ToPx2(Vector2 w) => new(ToPx(w.X), ToPx(w.Y));

        var info = new SKImageInfo(s, s, SKColorType.Rgba8888, SKAlphaType.Unpremul);
        using var bmp = new SKBitmap(info);
        using var canvas = new SKCanvas(bmp);
        canvas.Clear(SKColors.Transparent);

        var arrowColor = new SKColor(210, 210, 218, 230);
        var textColor = new SKColor(140, 140, 150, 235); // gray, not straight white
        using var fill = new SKPaint { Color = arrowColor, IsAntialias = true, Style = SKPaintStyle.Fill };
        using var stroke = new SKPaint { Color = arrowColor, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = s * 0.006f };
        var arrow = s * 0.05f; // arrow size in px

        // ---- FRONT arrow + label: at front-center (x=0, z=+fh), pointing toward the front (+Z, the
        //      camera-facing edge). Front is +Z in the right-handed canonical display basis. ----
        {
            var cx = ToPx(0f);
            var tipZ = fh + (e - fh) * 0.5f;       // a bit beyond the front edge, into the margin
            var tipY = ToPx(tipZ);
            var baseY = ToPx(fh - 0.4f);
            var halfWidth = arrow * 2f;            // double the arrow width
            using var path = new SKPath();
            path.MoveTo(cx, tipY);                 // apex (front, +Z)
            path.LineTo(cx - halfWidth, baseY);
            path.LineTo(cx + halfWidth, baseY);
            path.Close();
            canvas.DrawPath(path, fill);

            using var font = new SKFont(SKTypeface.Default, s * 0.06f);
            using var textPaint = new SKPaint { Color = textColor, IsAntialias = true };
            canvas.DrawText("FRONT", cx, baseY - s * 0.03f, SKTextAlign.Center, font, textPaint);
        }

        // ---- First-row arrow: at LED 0, pointing the way the first index advances. ----
        if (includeLed0Arrow)
        {
            using var font = new SKFont(SKTypeface.Default, s * 0.035f);
            using var textPaint = new SKPaint { Color = textColor, IsAntialias = true };
            var dirLen = firstAxisDir.Length();
            if (dirLen > 1e-3f)
            {
                var d = firstAxisDir / dirLen;
                var startPx = ToPx2(led0Floor + d * 0.3f);
                var endPx = ToPx2(led0Floor + d * 2.3f);
                canvas.DrawLine(startPx.X, startPx.Y, endPx.X, endPx.Y, stroke);
                // arrowhead
                var pd = new SKPoint(endPx.X - startPx.X, endPx.Y - startPx.Y);
                var pl = (float)System.Math.Sqrt(pd.X * pd.X + pd.Y * pd.Y);
                var ux = pd.X / pl; var uy = pd.Y / pl;
                using var head = new SKPath();
                head.MoveTo(endPx.X, endPx.Y);
                head.LineTo(endPx.X - arrow * ux + arrow * 0.7f * uy, endPx.Y - arrow * uy - arrow * 0.7f * ux);
                head.LineTo(endPx.X - arrow * ux - arrow * 0.7f * uy, endPx.Y - arrow * uy + arrow * 0.7f * ux);
                head.Close();
                canvas.DrawPath(head, fill);
                canvas.DrawText("LED 0", startPx.X, startPx.Y - arrow, SKTextAlign.Center, font, textPaint);
            }
            else
            {
                // First index runs vertically: just mark LED 0's column.
                var c = ToPx2(led0Floor);
                canvas.DrawCircle(c.X, c.Y, arrow * 0.8f, stroke);
                canvas.DrawText("LED 0", c.X, c.Y - arrow, SKTextAlign.Center, font, textPaint);
            }
        }

        canvas.Flush();
        return new Decal(bmp.Bytes, s);
    }
}
