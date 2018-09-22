namespace MapEditor.Controllers.MovementHandler
{
    // https://gamedevelopment.tutsplus.com/series/understanding-steering-behaviors--gamedev-12732
    public enum MovementMode
    {
        Move = 0,
        Follow,
        Intercept,
        Evade,
        Roam,
        Patrol,
    }
}
