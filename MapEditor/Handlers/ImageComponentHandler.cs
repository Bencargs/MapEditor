using System;
using System.Collections.Generic;
using System.Drawing;
using MapEditor.Entities;

namespace MapEditor.Handlers
{
    public class ImageComponentHandler : IComponentHandler
    {
        // todo: replace with an adbstraction class, seperate adding to draw queue and actually drawing
        private readonly Graphics _graphics;

        public ImageComponentHandler(Graphics graphics)
        {
            _graphics = graphics;
        }

        public IEnumerable<Type> RequiredComponents { get; }

        public void Handle(Entity entity, double elapsed)
        {
            // Does ordering of components matter? what if we update movement then render,
            // versus rendering then updating movement? how to we ensure consistency when 
            // components can be added or removed? sort by ComponentType in component handler?

            // Pass in relevant component data
            // todo: have each handler declare which component datas it requires passed to it?
            // how to validate handlers supported given entities components?

            // todo: refactor / remove ComponentController (extension class?)
            // Entity.GetComponent(ComponentType) ?
            //var componentData = (ImageComponent) entity.Components.FirstOrDefault(x => x.Type == ComponentType.RenderImage);

            // todo: pre-requisite on Position OR Area - how to handle boolean requisites?

            //_graphics.DrawImage(componentData.Image, );

            throw new NotImplementedException();
        }
    }
}
