using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using MapEditor.Common;
using MapEditor.Components;
using MapEditor.Engine;
using Newtonsoft.Json;

namespace MapEditor.Entities
{
    public static class UnitEx
    {
        public static T Clone<T>(this T source)
        {
            if (ReferenceEquals(source, null))
            {
                return default(T);
            }

            var serialized = JsonConvert.SerializeObject(source, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            });
            return JsonConvert.DeserializeObject<T>(serialized, new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                TypeNameHandling = TypeNameHandling.Objects
            });
        }

        public static Entity Clone(this Entity source)
        {
            var entity = new Entity();
            foreach (var c in source.Components)
            {
                entity.Components.Add(c.Clone());
            }
            var imageComponent = entity.GetComponent<ImageComponent>();
            if (imageComponent != null)
            {
                imageComponent.Image = source.GetComponent<ImageComponent>().Image;
            }
            return entity;
        }
    }

    public class AddUnitCommand : ICommand
    {
        public CommandType Id { get; } = CommandType.AddUnit;

        public Point Point { get; set; }
        public Entity Unit { get; set; }
    }

    public class NameComponent : IComponent
    {
        public string Title { get; set; }
        public string Name { get; set; }
    }

    public class UnitController : IHandleCommand
    {
        private readonly List<Entity> _units;
        private int _index;
        private const int Buffer = 1000;
        private readonly IGraphics _graphics;

        public UnitController(MessageHub messageHub, IGraphics graphics)
        {
            _graphics = graphics;
            _units = new List<Entity>();
            messageHub.Subscribe(this, CommandType.AddUnit);
        }

        public void Init()
        {
            for (var i = 0; i < Buffer; i++)
            {
                _units.Add(new Entity
                {
                    // todo: IdGenerator.NewId -- Determinate Id's based on a common game seed
                });
            }
        }

        /// <summary>
        /// Saves a unit Template
        /// </summary>
        public void SaveTemplate()
        {
            var entityImage = Image.FromFile(@"C:\Source\MapEditor\MapEditor\Units\Soldier\Images\bea468cd-fc00-32a3-0b1b-5a6f70869010.png");
            var entity = new Entity();
            entity.AddComponent(new NameComponent { Name = "Soldier" });
            entity.AddComponent(new PositionComponent { Position = new Vector2 { X = 0, Y = 0 } });
            entity.AddComponent(new PhysicsComponent { Mass = 5 });
            entity.AddComponent(new PathingComponent());
            entity.AddComponent(new ImageComponent
            {
                Id = new Guid(entityImage.GetImageHashcode()),
                Image = new Bitmap(entityImage)
            });
            entity.AddComponent(new MovementComponent
            {
                MovementMode = MovementMode.Move,
                TerrainTypes = new List<TerrainType> { TerrainType.Land },
                MaxVelocity = 10,
                Force = 5,
                SlowRadius = 5,
                StopRadius = 1
            });
            var json = JsonConvert.SerializeObject(entity, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            });
            File.WriteAllText(@"C:\Source\MapEditor\MapEditor\Units\Soldier\data.json", json);
            ZipFile.CreateFromDirectory(@"C:\Source\MapEditor\MapEditor\Units\Soldier", @"C:\Source\MapEditor\MapEditor\Units\soldier.unit");
        }

        /// <summary>
        /// Loans a unit template
        /// </summary>
        public Entity LoadTemplate(string filename)
        {
            using (var archive = ZipFile.OpenRead(filename))
            {
                var unitData = archive.GetEntry("data.json");
                if (unitData != null)
                {
                    Entity unit;
                    using (var stream = unitData.Open())
                    using (var reader = new StreamReader(stream))
                    {
                        var unitJson = reader.ReadToEnd();
                        unit = JsonConvert.DeserializeObject<Entity>(unitJson, new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.Objects
                        });
                    }

                    // todo: foreach and load multiple images to populate an animation component
                    var imageComponent = unit.GetComponent<ImageComponent>();
                    if (imageComponent != null)
                    {
                        var imageData = archive.GetEntry($"Images/{imageComponent.Id}.png");
                        if (imageData != null)
                        {
                            using (var stream = imageData.Open())
                            using (var image = Image.FromStream(stream))
                            {
                                imageComponent.Image = new Bitmap(image);
                                imageComponent.Image.MakeTransparent(Color.Fuchsia);
                            }
                        }
                    }
                    return unit;
                }
            }
            return null;
        }

        public void AddUnit(Point point, Entity unit)
        {
            var position = unit.GetComponent<PositionComponent>();
            position.Position = new Vector2 {X = point.X, Y = point.Y};
            // todo: sanitise this Point / Vector insanity

            if (_index > Buffer)
                throw new Exception("Too many units");  //todo: handle this nicely - DisplayErrorCommand ?

            _units[_index++] = unit.Clone();
        }

        //public void Update()
        //{

        //}

        public void Render()
        {
            // todo: camera.Contains

            foreach (var u in _units)
            {
                var imageComponent = u.GetComponent<ImageComponent>();
                if (imageComponent != null)
                {
                    var positionComponent = u.GetComponent<PositionComponent>();
                    var image = imageComponent.Image;   //todo: these are use a lot - assuming all units have an image and position, extension methods?
                    var position = positionComponent.Position;
                    var area = new Rectangle(position.X, position.Y, image.Width, image.Height);
                    _graphics.DrawImage(image, area);
                }
            }
        }

        public void Handle(ICommand command)
        {
            switch (command)
            {
                case AddUnitCommand c:
                    // todo: change signiture to only accept unit, position should be concern of the unit
                    AddUnit(c.Point, c.Unit);
                    break;
            }
        }

        public void Undo(ICommand command)
        {
            throw new NotImplementedException();
        }
    }
}
