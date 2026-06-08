namespace CindarsHope.Farm.Buildings
{
    /// <summary>
    /// Construction request for a new building.
    /// Immutable input for construction job creation.
    /// </summary>
    public class ConstructionRequest
    {
        /// <summary>
        /// Building definition being constructed.
        /// </summary>
        public FarmBuildingDefinition Building { get; }

        /// <summary>
        /// Placement position X.
        /// </summary>
        public float PositionX { get; }

        /// <summary>
        /// Placement position Y.
        /// </summary>
        public float PositionY { get; }

        /// <summary>
        /// Rotation (0-3, representing 0/90/180/270 degrees). Only used if building CanRotate.
        /// </summary>
        public int Rotation { get; }

        public ConstructionRequest(FarmBuildingDefinition building, float posX, float posY, int rotation = 0)
        {
            Building = building;
            PositionX = posX;
            PositionY = posY;
            Rotation = rotation % 4;
        }
    }
}
