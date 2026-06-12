namespace LedCube.Core.UI.CubeView3D.Rendering;

/// <summary>
/// GLSL ES 3.00 shaders — Avalonia uses ANGLE (GL ES) on Windows, so the version directive and
/// precision qualifiers are required. Matrices are uploaded raw from <c>System.Numerics</c>
/// (row-vector) and multiplied on the left here; the column-major/row-vector conventions cancel,
/// so no transpose is needed on upload.
/// </summary>
internal static class Shaders
{
    // ---- Instanced LED mesh ----
    public const string LedVertex = """
        #version 300 es
        precision highp float;

        layout(location = 0) in vec3 aPos;
        layout(location = 1) in vec3 aNormal;
        layout(location = 2) in vec3 aInstancePos;   // per-instance grid position (world)
        layout(location = 3) in float aBrightness;    // per-instance 0..1

        uniform mat4 uViewProj;
        uniform mat4 uModel;   // shared rotation + uniform scale (no translation)

        out vec3 vNormal;
        out float vBrightness;

        void main()
        {
            vec3 local = (uModel * vec4(aPos, 1.0)).xyz;
            vec3 world = local + aInstancePos;
            gl_Position = uViewProj * vec4(world, 1.0);
            vNormal = normalize((uModel * vec4(aNormal, 0.0)).xyz);
            vBrightness = aBrightness;
        }
        """;

    public const string LedFragment = """
        #version 300 es
        precision highp float;

        in vec3 vNormal;
        in float vBrightness;

        out vec4 fragColor;

        uniform vec3 uLightDir;   // normalized, world space
        uniform vec3 uOnColor;    // emissive/body color when lit
        uniform vec3 uOffColor;   // body color when off
        uniform float uAmbient;
        uniform float uOnAlpha;   // alpha of a lit LED
        uniform float uOffAlpha;  // alpha of an off LED (low -> see through)

        void main()
        {
            vec3 n = normalize(vNormal);
            float diff = max(dot(n, -uLightDir), 0.0);
            vec3 body = mix(uOffColor, uOnColor, vBrightness);
            vec3 lit = body * (uAmbient + (1.0 - uAmbient) * diff);
            vec3 color = lit + uOnColor * vBrightness * 0.6; // emissive glow when on
            float alpha = mix(uOffAlpha, uOnAlpha, vBrightness);
            fragColor = vec4(color, alpha);
        }
        """;

    // ---- Solid lit mesh (podest) ----
    public const string SolidVertex = """
        #version 300 es
        precision highp float;

        layout(location = 0) in vec3 aPos;
        layout(location = 1) in vec3 aNormal;

        uniform mat4 uViewProj;
        uniform mat4 uModel;

        out vec3 vNormal;

        void main()
        {
            gl_Position = uViewProj * (uModel * vec4(aPos, 1.0));
            vNormal = normalize((uModel * vec4(aNormal, 0.0)).xyz);
        }
        """;

    public const string SolidFragment = """
        #version 300 es
        precision highp float;

        in vec3 vNormal;
        out vec4 fragColor;

        uniform vec3 uColor;
        uniform vec3 uLightDir;
        uniform float uAmbient;

        void main()
        {
            float diff = max(dot(normalize(vNormal), -uLightDir), 0.0);
            vec3 c = uColor * (uAmbient + (1.0 - uAmbient) * diff);
            fragColor = vec4(c, 1.0);
        }
        """;

    // ---- Textured quad (floor decal: arrows + FRONT label) ----
    public const string TexturedVertex = """
        #version 300 es
        precision highp float;

        layout(location = 0) in vec3 aPos;
        layout(location = 1) in vec2 aUv;

        uniform mat4 uViewProj;
        uniform mat4 uModel;

        out vec2 vUv;

        void main()
        {
            gl_Position = uViewProj * (uModel * vec4(aPos, 1.0));
            vUv = aUv;
        }
        """;

    public const string TexturedFragment = """
        #version 300 es
        precision highp float;

        in vec2 vUv;
        out vec4 fragColor;

        uniform sampler2D uTex;
        uniform float uOpacity;

        void main()
        {
            vec4 t = texture(uTex, vUv);
            fragColor = vec4(t.rgb, t.a * uOpacity);
        }
        """;
}
