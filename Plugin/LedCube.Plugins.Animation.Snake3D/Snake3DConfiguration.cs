namespace LedCube.Plugins.Animation.Snake3D;

public class Snake3DConfiguration
{
    public const string SectionName = "Snake3D";

    public EdgeBehaviour EdgeBehaviour { get; set; } = EdgeBehaviour.GameOver;

    public float FoodGrowthFactor { get; set; } = 1.0f;

    public int ActiveFoodCount { get; set; } = 1;
}

public enum EdgeBehaviour
{
    GameOver,
    RollOver
}