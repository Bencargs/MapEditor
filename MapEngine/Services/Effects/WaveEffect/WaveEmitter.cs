using System;
using Common;
using Common.Entities;
using MapEngine.Entities;
using MapEngine.Services.Map;

namespace MapEngine.Services.Effects.WaveEffect;

public class WaveEmitter
{
    public Entity Entity { get; set; }
    
    // todo: from config or something? size or mass of entity?
    public int Strength { get; set; } = 1000;

    private int _spawnRate = 1000;
    private DateTime _previousSpawn = DateTime.Now;

    public bool ShouldEmit()
    {
        if (!Entity.IsMoving())
            return false;
        
        var elapsed = (DateTime.Now - _previousSpawn).TotalMilliseconds;
        if (elapsed < _spawnRate)
            return false;
        
        _previousSpawn = DateTime.Now;
        return true;
    }
}