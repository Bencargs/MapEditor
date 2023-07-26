using Common;
using Common.Collision;
using Common.Entities;
using MapEngine.Entities.Components;
using MapEngine.Factories;
using MapEngine.Handlers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace MapEngine.ResourceLoading
{
    public static class UnitLoader
    {
        public static Entity[] LoadUnits(string mapFile)
        {
            var json = File.ReadAllText(mapFile);
            dynamic mapData = JsonConvert.DeserializeObject(json);

            var units = ((IEnumerable<dynamic>)mapData.Units).Select(u =>
            {
                //Eg. flyweight pattern
                UnitFactory.TryGetUnit((string)u.Type, out var prototype);
                var entity = Clone((int)u.Id, prototype);

                var location = u.Location;
                if (location != null)
                {
                    var locationComponent = entity.GetComponent<LocationComponent>();
                    locationComponent.Location = new Vector2((int)location.X, (int)location.Y);
                    locationComponent.FacingAngle = (float)(location.FacingAngle ?? 0);
                }

                var team = u.Team;
                if (team != null)
                {
                    entity.GetComponent<UnitComponent>().TeamId = team;
                }

                var movement = u.Movement;
                if (movement != null)
                {
                    var movementComponent = entity.GetComponent<MovementComponent>();
                    if (movementComponent != null)
                    {
                        movementComponent.Velocity = new Vector3((int)movement.Velocity.X, (int)movement.Velocity.Y, 0);
                        movementComponent.Steering = new Vector2((int)movement.Steering.X, (int)movement.Steering.Y);
                        movementComponent.Destinations = (movement.Destinations != null)
                            ? ((IEnumerable<dynamic>)movement.Destinations).Select(x => new MoveOrder
                            {
                                MovementMode = (MovementMode)Enum.Parse(typeof(MovementMode), (string)x.MovementMode),
                                Destination = new Vector2((int)x.Destination.X, (int)x.Destination.Y)
                            }).ToQueue()
                            : new Queue<MoveOrder>();
                    }
                }

                return entity;
            }).ToArray();

            return units;
        }

        public static Entity LoadUnitDefinition(string filename)
        {
            var json = File.ReadAllText(filename);
            dynamic unitData = JsonConvert.DeserializeObject(json);

            var entity = new Entity();

            entity.AddComponent(new LocationComponent());

            entity.AddComponent(new UnitComponent
            {
                UnitType = (string)unitData.Type
            });

            var image = unitData.Image;
            if (image != null)
            {
                entity.AddComponent(new ImageComponent
                {
                    TextureId = (string)image.TextureId
                });
            }

            var model = unitData.Model;
            if (model != null)
            {
                entity.AddComponent(new ModelComponent
                {
                    ModelId = (string)model.ModelId
                });
            }

            var movement = unitData.Movement;
            if (movement != null)
            {
                entity.AddComponent(new MovementComponent
                {
                    Velocity = Vector3.Zero,
                    Steering = Vector2.Zero,
                    MaxVelocity = (float)movement.MaxVelocity,
                    Mass = (float)(movement.Mass ?? 1),
                    MaxForce = (float)movement.MaxForce,
                    StopRadius = (float)movement.StopRadius,
                    BrakeForce = (float)movement.BrakeForce,
                    Terrains = movement.Terrains != null 
                        ? ((IEnumerable<dynamic>)movement.Terrains).Select(x => (TerrainType)x).ToArray()
                        : DefaultTerrain,
                    MovementMask = movement.MovementMask != null
                        ? ((IEnumerable<dynamic>)unitData.Movement.MovementMask).Select(x =>
                            ((IEnumerable<dynamic>)x).Select(y => ((int)y.Item1, (int)y.Item2)))
                            .To2DArray()
                        : DefaultMask
                });
            }

            var sensors = (IEnumerable<dynamic>)unitData.Sensors;
            if (sensors != null)
            {
                foreach (var s in sensors)
                {
                    entity.AddComponent(new SensorComponent
                    {
                        Name = s.Name,
                        Radius = s.Radius
                    });
                }
            }

            var particles = (IEnumerable<dynamic>) unitData.Particles;
            if (particles != null)
            {
                foreach (var p in particles)
                {
                    string particleType = p.Type;
                    if (!ParticleFactory.TryGetParticle(particleType, out var particle))
                        continue;
                    
                    entity.AddComponent(particle);
                }
            }

            var area = unitData.Area;
            if (area != null)
            {
                ICollider collider = null;
                if (area.BoundingBox != null)
                {
                    collider = new BoundingBox
                    {
                        Width = (int)area.BoundingBox.Width,
                        Height = (int)area.BoundingBox.Height
                    };
                }
                entity.AddComponent(new CollisionComponent(collider));
            }

            var weapons = unitData.Weapons;
            if (weapons != null)
            {
                foreach (string weaponId in (IEnumerable<dynamic>) weapons)
                {
                    if (!WeaponFactory.TryGetWeapon(weaponId, out var weapon))
                        continue;

                    entity.AddComponent(weapon);
                }
            }

            return entity;
        }

        private static readonly (int, int)[,] DefaultMask = {
			{ (-1, -1), (0, -1), (1, -1) },
            { (-1,  0), (0,  0), (1,  0) },
            { (-1,  1), (0,  1), (1,  1) }
        };

        private static readonly TerrainType[] DefaultTerrain = new[] { TerrainType.Land };

        // todo: move this to an entity extension class?
        // or put it on entity class?
        private static Entity Clone(int id, Entity entity)
        {
            var clone = new Entity();
            foreach (var component in entity.Components)
            {
                clone.Components.Add(component.Clone());
            }
            clone.Id = id;
            return clone;
        }
    }
}
