using Common.Entities;
using MapEngine.Entities.Components;
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
        public static Entity[] LoadUnits(string mapFile, Dictionary<string, Entity> definitions)
        {
            var json = File.ReadAllText(mapFile);
            dynamic mapData = JsonConvert.DeserializeObject(json);

            var units = ((IEnumerable<dynamic>)mapData.Units).Select(u =>
            {
                //Eg. flyweight pattern
                var entity = Clone(definitions[(string)u.Type]);
                entity.Id = (int)u.Id;

                var location = u.Location;
                if (location != null)
                {
                    entity.GetComponent<LocationComponent>().Location = new Vector2((int)location.X, (int)location.Y);
                }

                var team = u.Team;
                if (team != null)
                {
                    entity.GetComponent<UnitComponent>().TeamId = team;
                }

                var movement = u.Movement;
                if (movement != null)
                {
                    entity.AddComponent(new MovementComponent
                    {
                        FacingAngle = (int)movement.FacingAngle,
                        Velocity = new Vector2((int)movement.Velocity.X, (int)movement.Velocity.Y),
                        Steering = new Vector2((int)movement.Steering.X, (int)movement.Steering.Y),
                        MaxVelocity = (float)movement.MaxVelocity,
                        Mass = (float)movement.Mass,
                        MaxForce = (float)movement.MaxForce,
                        StopRadius = (float)movement.StopRadius,
                        Destinations = new Queue<MoveOrder>(((IEnumerable<dynamic>)movement.Destinations).Select(x => new MoveOrder
                        {
                            MovementMode = (MovementMode)Enum.Parse(typeof(MovementMode), (string)x.MovementMode),
                            Destination = new Vector2((int)x.Destination.X, (int)x.Destination.Y)
                        }))
                    });
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

            // every unit should have a physical location I think
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

            return entity;
        }

        private static Entity Clone(Entity entity)
        {
            var clone = new Entity();
            foreach (var component in entity.Components)
            {
                clone.Components.Add(component.Clone());
            }
            return clone;
        }
    }
}
