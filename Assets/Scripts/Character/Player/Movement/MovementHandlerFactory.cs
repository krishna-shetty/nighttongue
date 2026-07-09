using UnityEngine;
using static MovementHandlerType;

public enum MovementHandlerType
{
    AIR,
    GROUNDED,
    FRICTIONLESS,
    GRAPPLING,
    SWINGING
}

public class MovementHandlerFactory
{
    // Whenever a new handler is made, add it to the enum and the return case here,
    // then simply call GetHandler() and store the handler/call Apply() when needed
    // ** note: should GetHandler() return a whole object or should Apply() be made static
    //          and this function made to call the appropriate Apply()?
    public static MovementHandlerBase GetHandler(MovementHandlerType type, PlayerController controller)
    {
        return type switch
        {
            GROUNDED => new GroundedMovementHandler(controller),
            AIR => new AirMovementHandler(controller),
            FRICTIONLESS => new GroundedMovementHandler(controller),
            GRAPPLING => new GrapplingMovementHandler(controller),
            SWINGING => new SwingingMovementHandler(controller),
            _ => null
        };
    }
}