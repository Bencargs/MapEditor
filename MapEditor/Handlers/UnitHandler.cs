using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Common;
using MapEditor.Commands;
using MapEditor.Common;
using MapEditor.Components;
using MapEditor.Controllers.MovementHandler;
using MapEditor.Engine;
using MapEditor.Entities;
using MapEditor.Handlers.CollisionHandler;
using MapEditor.Repository;
using Newtonsoft.Json;
using Rectangle = System.Drawing.Rectangle;

namespace MapEditor.Handlers
{
    public class UnitHandler : IHandleCommand
    {
        private readonly MessageHub _messageHub;
        private readonly ISession _session;
        private const int Buffer = 1000;
        private int _index = 0;
        private List<Entity> SelectedUnits { get; set; } = new List<Entity>();

        public UnitHandler(MessageHub messageHub, ISession session)
        {
            _messageHub = messageHub;
            _session = session;
        }

        public void Init()
        {
            _messageHub.Subscribe(this, CommandType.AddUnit);
            _messageHub.Subscribe(this, CommandType.SelectUnits);
        }

        /// <summary>
        /// Saves a unit Template
        /// </summary>
        public void SaveTemplate()
        {
            var entityImage = Image.FromFile(@"C:\Source\MapEditor\MapEditor\Units\Images\bea468cd-fc00-32a3-0b1b-5a6f70869010.png");
            var entity = new Entity();
            entity.AddComponent(new UnitComponent { Name = "Soldier" });
            entity.AddComponent(new PositionComponent { Position = new Vector2 { X = 0, Y = 0 } }); //todo: do we need position if we have a collider?
            entity.AddComponent(new PhysicsComponent { Mass = 5 });
            entity.AddComponent(new CollisionComponent
            {
                Collider = new BoundingBox
                {
                    // todo: constructor?
                    Width = entityImage.Width,
                    Height = entityImage.Height,
                    Position = new Point {X = 0, Y = 0}
                }
            });
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
            var json = JsonConvert.SerializeObject(entity);
            System.IO.File.WriteAllText(@"C:\Source\MapEditor\MapEditor\Units\data.json", json);
            ZipFile.CreateFromDirectory(@"C:\Source\MapEditor\MapEditor\Units\", @"C:\Source\MapEditor\MapEditor\soldier.unit");
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
                        unit = JsonConvert.DeserializeObject<Entity>(unitJson);
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

            var units = _session.GetUnits(point);
            if (units
                .Select(x => x.GetComponent<CollisionComponent>())
                .Where(x => x != null)
                .Select(x => x.Collider)
                .Any())
            {
                return;
            }

            var newUnit = unit.Clone();
            units.Add(newUnit);
        }

        public void SelectUnits(Rectangle area)
        {
            foreach (var unit in SelectedUnits.ToList())
            {
                unit.GetComponent<UnitComponent>().IsSelected = false;
                SelectedUnits.Remove(unit);
            }

            var units = _session.GetUnits(area);
            foreach (var unit in units)
            {
                var selectionComponent = unit.GetComponent<UnitComponent>();    //todo: replace with get components
                selectionComponent.IsSelected = true;
                SelectedUnits.Add(unit);
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
                case SelectUnitsCommand c:
                    SelectUnits(c.Area);
                    break;
            }
        }

        public void Undo(ICommand command)
        {
            throw new NotImplementedException();
        }
    }
}
