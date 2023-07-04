using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Shapes;
using Common;
using MapEngine.Entities;
using MapEngine.Entities.Components;
using MapEngine.Extensions;
using Rectangle = Common.Rectangle;

namespace MapEngine.Handlers
{
    public class InterfaceHandler
    {
        private readonly InputState _inputState;

        public InterfaceHandler(InputState inputState)
        {
            _inputState = inputState;
        }

        public void Render(Rectangle viewport, IGraphics graphics)
        {
            var buffer = new byte[768 * 512 * 4]; // todo: dimentions from map

            DrawSelectionBox(buffer);

            foreach (var selected in _inputState.SelectedEntities)
            {
                var angle = selected.GetComponent<LocationComponent>().FacingAngle;
                var area = selected.Texture();
                var location = selected.Location();
                var centeredLocation = new Vector2(location.X - (area.Width / 2), location.Y - (area.Height / 2));
                DrawBoxOnImage(centeredLocation, area.Width + 2, area.Height + 2, angle, buffer, 768);
                //DrawRoundedBoxOnImage(centeredLocation, area.Width + 2, area.Height + 2, 4, angle, buffer, 768);
                //DrawRoundedBoxOutline(centeredLocation, area.Width + 4, area.Height + 4, 8, buffer, 768);
            }

           // DrawSelectedEntities(buffer);

            graphics.DrawBytes(buffer, viewport);
        }

        private void DrawSelectionBox(byte[] buffer)
        {
            if (_inputState.SelectionStart is null) return;

            var startX = (int)Math.Min(_inputState.SelectionStart.Value.X, _inputState.Location.X).Clamp(0, 768);
            var startY = (int)Math.Min(_inputState.SelectionStart.Value.Y, _inputState.Location.Y).Clamp(0, 768);
            var endX = (int)Math.Max(_inputState.SelectionStart.Value.X, _inputState.Location.X).Clamp(0, 768);
            var endY = (int)Math.Max(_inputState.SelectionStart.Value.Y, _inputState.Location.Y).Clamp(0, 768);

            var bytesPerPixel = 4; // RGBA format
            var stride = 768 * bytesPerPixel;

            for (var y = startY; y < endY; y++)
            {
                for (var x = startX; x < endX; x++)
                {
                    var pixelIndex = (y * stride) + (x * bytesPerPixel);

                    if (x == startX || x == endX - 1 || y == startY || y == endY - 1)
                    {
                        // Highlight perimeter
                        buffer[pixelIndex] = 0; // Red component
                        buffer[pixelIndex + 1] = 255; // Green component
                        buffer[pixelIndex + 2] = 0; // Blue component
                        buffer[pixelIndex + 3] = 255; // Alpha component
                    }
                    else
                    {
                        // Fill interior
                        buffer[pixelIndex] = 255; // Red component
                        buffer[pixelIndex + 1] = 255; // Green component
                        buffer[pixelIndex + 2] = 255; // Blue component
                        buffer[pixelIndex + 3] = 25; // Alpha component
                    }
                }
            }

            //graphics.DrawBytes(buffer, viewport);
        }

        public void DrawSelectedEntities(byte[] buffer)
        {
            var imageWidth = 768;
            var colour = new Colour(0, 0, 255, 255);

            foreach (var entity in _inputState.SelectedEntities)
            {
                var location = entity.Location();
                var texture = entity.Texture();
                var radius = (int)(Math.Max(texture.Width, texture.Height) / 2) + 4;

                var centerX = (int)location.X;
                var centerY = (int)location.Y;
                int bytesPerPixel = 4; // RGBA format
                int stride = imageWidth * bytesPerPixel;

                int x = radius;
                int y = 0;
                int radiusError = 1 - x;

                while (x >= y)
                {
                    SetPixel(buffer, GetIndex(centerX + x, centerY + y, stride, bytesPerPixel), colour);
                    SetPixel(buffer, GetIndex(centerX - x, centerY + y, stride, bytesPerPixel), colour);
                    SetPixel(buffer, GetIndex(centerX + x, centerY - y, stride, bytesPerPixel), colour);
                    SetPixel(buffer, GetIndex(centerX - x, centerY - y, stride, bytesPerPixel), colour);
                    SetPixel(buffer, GetIndex(centerX + y, centerY + x, stride, bytesPerPixel), colour);
                    SetPixel(buffer, GetIndex(centerX - y, centerY + x, stride, bytesPerPixel), colour);
                    SetPixel(buffer, GetIndex(centerX + y, centerY - x, stride, bytesPerPixel), colour);
                    SetPixel(buffer, GetIndex(centerX - y, centerY - x, stride, bytesPerPixel), colour);

                    y++;

                    if (radiusError < 0)
                    {
                        radiusError += 2 * y + 1;
                    }
                    else
                    {
                        x--;
                        radiusError += 2 * (y - x) + 1;
                    }
                }
            }
        }

        public static void DrawRoundedBoxOutline(Vector2 location, int width, int height, int cornerRadius, byte[] image, int imageWidth)
        {
            int startX = (int)location.X;
            int startY = (int)location.Y;
            int endX = startX + width;
            int endY = startY + height;

            int bytesPerPixel = 4; // RGBA format
            int stride = imageWidth * bytesPerPixel;

            // Calculate the coordinates for the rounded corners
            int topLeftX = startX + cornerRadius;
            int topLeftY = startY + cornerRadius;
            int topRightX = endX - cornerRadius;
            int topRightY = startY + cornerRadius;
            int bottomLeftX = startX + cornerRadius;
            int bottomLeftY = endY - cornerRadius;
            int bottomRightX = endX - cornerRadius;
            int bottomRightY = endY - cornerRadius;

            // Draw the top and bottom lines
            for (int x = startX + cornerRadius; x <= endX - cornerRadius; x++)
            {
                int topLineY = startY;
                int bottomLineY = endY;

                // Check if we are within the rounded corner area
                if (x >= topLeftX && x <= topRightX)
                {
                    topLineY = topLeftY - cornerRadius;
                    bottomLineY = bottomLeftY + cornerRadius;
                }

                // Draw the top line
                int topLinePixelIndex = (topLineY * stride) + (x * bytesPerPixel);
                image[topLinePixelIndex] = 0;       // Red component
                image[topLinePixelIndex + 1] = 255; // Green component
                image[topLinePixelIndex + 2] = 0;   // Blue component
                image[topLinePixelIndex + 3] = 255; // Alpha component

                // Draw the bottom line
                int bottomLinePixelIndex = (bottomLineY * stride) + (x * bytesPerPixel);
                image[bottomLinePixelIndex] = 0;       // Red component
                image[bottomLinePixelIndex + 1] = 255; // Green component
                image[bottomLinePixelIndex + 2] = 0;   // Blue component
                image[bottomLinePixelIndex + 3] = 255; // Alpha component
            }

            // Draw the left and right lines
            for (int y = startY + cornerRadius; y <= endY - cornerRadius; y++)
            {
                int leftLineX = startX;
                int rightLineX = endX;

                // Check if we are within the rounded corner area
                if (y >= topLeftY && y <= bottomLeftY)
                {
                    leftLineX = topLeftX - cornerRadius;
                    rightLineX = topRightX + cornerRadius;
                }

                // Draw the left line
                int leftLinePixelIndex = (y * stride) + (leftLineX * bytesPerPixel);
                image[leftLinePixelIndex] = 0;       // Red component
                image[leftLinePixelIndex + 1] = 255; // Green component
                image[leftLinePixelIndex + 2] = 0;   // Blue component
                image[leftLinePixelIndex + 3] = 255; // Alpha component

                // Draw the right line
                int rightLinePixelIndex = (y * stride) + (rightLineX * bytesPerPixel);
                image[rightLinePixelIndex] = 0;       // Red component
                image[rightLinePixelIndex + 1] = 255; // Green component
                image[rightLinePixelIndex + 2] = 0;   // Blue component
                image[rightLinePixelIndex + 3] = 255; // Alpha component
            }

            // Draw the rounded corners
            DrawRoundedCorner(topLeftX, topLeftY, cornerRadius, image, stride);
            DrawRoundedCorner(topRightX, topRightY, cornerRadius, image, stride);
            DrawRoundedCorner(bottomLeftX, bottomLeftY, cornerRadius, image, stride);
            DrawRoundedCorner(bottomRightX, bottomRightY, cornerRadius, image, stride);
        }

        private static void DrawRoundedCorner(int centerX, int centerY, int radius, byte[] image, int stride)
        {
            int bytesPerPixel = 4; // RGBA format

            int x = radius;
            int y = 0;
            int radiusError = 1 - x;

            while (x >= y)
            {
                // Calculate the pixel indices for each quadrant
                int topLeftPixelIndex = ((centerY - y) * stride) + ((centerX - x) * bytesPerPixel);
                int topRightPixelIndex = ((centerY - y) * stride) + ((centerX + x) * bytesPerPixel);
                int bottomLeftPixelIndex = ((centerY + y) * stride) + ((centerX - x) * bytesPerPixel);
                int bottomRightPixelIndex = ((centerY + y) * stride) + ((centerX + x) * bytesPerPixel);

                // Set the pixel color for each quadrant
                image[topLeftPixelIndex] = 0;       // Red component
                image[topLeftPixelIndex + 1] = 0;   // Green component
                image[topLeftPixelIndex + 2] = 255; // Blue component
                image[topLeftPixelIndex + 3] = 255; // Alpha component

                image[topRightPixelIndex] = 0;       // Red component
                image[topRightPixelIndex + 1] = 0;   // Green component
                image[topRightPixelIndex + 2] = 255; // Blue component
                image[topRightPixelIndex + 3] = 255; // Alpha component

                image[bottomLeftPixelIndex] = 0;       // Red component
                image[bottomLeftPixelIndex + 1] = 0;   // Green component
                image[bottomLeftPixelIndex + 2] = 255; // Blue component
                image[bottomLeftPixelIndex + 3] = 255; // Alpha component

                image[bottomRightPixelIndex] = 0;       // Red component
                image[bottomRightPixelIndex + 1] = 0;   // Green component
                image[bottomRightPixelIndex + 2] = 255; // Blue component
                image[bottomRightPixelIndex + 3] = 255; // Alpha component

                y++;

                if (radiusError < 0)
                {
                    radiusError += 2 * y + 1;
                }
                else
                {
                    x--;
                    radiusError += 2 * (y - x + 1);
                }
            }
        }


        public static void DrawRoundedBoxOnImage(Vector2 location, int width, int height, int cornerRadius, float facingAngle, byte[] image, int imageWidth)
        {
            int startX = (int)location.X;
            int startY = (int)location.Y;
            int endX = startX + width;
            int endY = startY + height;

            // Calculate center point of the box
            float centerX = startX + (width / 2f);
            float centerY = startY + (height / 2f);

            int bytesPerPixel = 4; // RGBA format
            int stride = imageWidth * bytesPerPixel;

            // Calculate circle radius
            var circleRadius = Math.Min((width / 2) + cornerRadius, (height / 2) + cornerRadius);

            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    bool isOnCirclePerimeter = IsPointOnCirclePerimeter(x, y, centerX, centerY, circleRadius);
                    bool isOnRoundedBoxPerimeter = IsPointOnRoundedBoxPerimeter(x, y, startX, startY, endX, endY, cornerRadius);

                    if (isOnRoundedBoxPerimeter)
                    {
                        // Draw box outline
                        //int pixelIndex = (y * stride) + (x * bytesPerPixel);
                        Vector2 rotatedPoint = RotatePoint(new Vector2(x, y), new Vector2(centerX, centerY), facingAngle);
                        int pixelIndex = ((int)rotatedPoint.Y * stride) + ((int)rotatedPoint.X * bytesPerPixel);

                        image[pixelIndex] = 0;       // Red component
                        image[pixelIndex + 1] = 255; // Green component
                        image[pixelIndex + 2] = 0;   // Blue component
                        image[pixelIndex + 3] = 255; // Alpha component
                    }
                    else if (isOnCirclePerimeter)
                    {
                        // Draw circle outline
                        //int pixelIndex = (y * stride) + (x * bytesPerPixel);
                        Vector2 rotatedPoint = RotatePoint(new Vector2(x, y), new Vector2(centerX, centerY), facingAngle);
                        int pixelIndex = ((int)rotatedPoint.Y * stride) + ((int)rotatedPoint.X * bytesPerPixel);

                        image[pixelIndex] = 0;       // Red component
                        image[pixelIndex + 1] = 255;   // Green component
                        image[pixelIndex + 2] = 0; // Blue component
                        image[pixelIndex + 3] = 255; // Alpha component
                    }
                }
            }
        }

        private static bool IsPointOnCirclePerimeter(int x, int y, float centerX, float centerY, float radius)
        {
            int deltaX = x - (int)centerX;
            int deltaY = y - (int)centerY;
            int distanceSquared = (deltaX * deltaX) + (deltaY * deltaY);
            var radiusSquared = radius * radius;

            return distanceSquared >= (radiusSquared - 1) && distanceSquared <= (radiusSquared + 1);
        }

        private static bool IsPointOnRoundedBoxPerimeter(int x, int y, int startX, int startY, int endX, int endY, int cornerRadius)
        {
            int left = startX;
            int right = endX;
            int top = startY;
            int bottom = endY;

            // Check if point is on the top or bottom edge
            if ((x >= (startX + cornerRadius) && x <= (right - cornerRadius)) && (y == top || y == bottom))
                return true;

            // Check if point is on the left or right edge
            if ((y >= (top + cornerRadius) && y <= (bottom - cornerRadius)) && (x == left || x == right))
                return true;

            return false;
        }

        //public static void DrawRoundedBoxOnImage(Vector2 location, int width, int height, int cornerRadius, float facingAngle, byte[] image, int imageWidth)
        //{
        //    int startX = (int)location.X;
        //    int startY = (int)location.Y;
        //    int endX = startX + width;
        //    int endY = startY + height;

        //    // Calculate center point of the box
        //    float centerX = startX + (width / 2f);
        //    float centerY = startY + (height / 2f);

        //    int bytesPerPixel = 4; // RGBA format
        //    int stride = imageWidth * bytesPerPixel;

        //    for (int y = startY; y <= endY; y++)
        //    {
        //        for (int x = startX; x <= endX; x++)
        //        {
        //            if (IsPointOnRoundedBoxPerimeter(x, y, startX, startY, endX, endY, cornerRadius))
        //            {
        //                // Rotate point around the center
        //                Vector2 rotatedPoint = RotatePoint(new Vector2(x, y), new Vector2(centerX, centerY), facingAngle);

        //                int pixelIndex = ((int)rotatedPoint.Y * stride) + ((int)rotatedPoint.X * bytesPerPixel);
        //                image[pixelIndex] = 0;       // Red component
        //                image[pixelIndex + 1] = 255; // Green component
        //                image[pixelIndex + 2] = 0;   // Blue component
        //                image[pixelIndex + 3] = 255; // Alpha component
        //            }
        //        }
        //    }
        //}

        //private static bool IsPointOnRoundedBoxPerimeter(int x, int y, int startX, int startY, int endX, int endY, int cornerRadius)
        //{
        //    int left = startX;
        //    int right = endX;
        //    int top = startY;
        //    int bottom = endY;

        //    // Check if point is on the top or bottom edge
        //    if ((x >= (startX + cornerRadius) && x <= (right - cornerRadius)) && (y == top || y == bottom))
        //        return true;

        //    // Check if point is on the left or right edge
        //    if ((y >= (top + cornerRadius) && y <= (bottom - cornerRadius)) && (x == left || x == right))
        //        return true;

        //    // Calculate the center of the box
        //    int centerX = startX + ((endX - startX) / 2);
        //    int centerY = startY + ((endY - startY) / 2);

        //    int boxWidth = endX - startX;
        //    int boxHeight = endY - startY;

        //    int circleRadius = (boxWidth / 2) + cornerRadius;

        //    // Check if point is on the circular outline
        //    int deltaX = x - centerX;
        //    int deltaY = y - centerY;
        //    int distanceSquared = (deltaX * deltaX) + (deltaY * deltaY);

        //    return distanceSquared >= ((circleRadius - 1) * (circleRadius - 1)) && distanceSquared <= ((circleRadius + 1) * (circleRadius + 1));
        //}

        private static bool IsPointInsideCircle(int x, int y, Vector2 center, int radius)
        {
            int deltaX = x - (int)center.X;
            int deltaY = y - (int)center.Y;
            int distanceSquared = (deltaX * deltaX) + (deltaY * deltaY);
            int radiusSquared = radius * radius;

            return distanceSquared <= radiusSquared;
        }

        private static bool IsPointInsideBox(int x, int y, int left, int top, int right, int bottom)
        {
            return x >= left && x <= right && y >= top && y <= bottom;
        }

        private static bool IsPointOnRoundedCornerPerimeter(int x, int y, int cornerX, int cornerY, int cornerRadius)
        {
            int deltaX = x - cornerX;
            int deltaY = y - cornerY;
            int distanceSquared = (deltaX * deltaX) + (deltaY * deltaY);
            int radiusSquared = cornerRadius * cornerRadius;

            // Check if the point is within the quadrant of the curved corner
            if (deltaX >= 0 && deltaY >= 0)
            {
                // Top-left quadrant
                return distanceSquared >= radiusSquared && deltaX <= cornerRadius && deltaY <= cornerRadius;
            }
            else if (deltaX >= 0 && deltaY < 0)
            {
                // Bottom-left quadrant
                return distanceSquared >= radiusSquared && deltaX <= cornerRadius && deltaY >= -cornerRadius;
            }
            else if (deltaX < 0 && deltaY >= 0)
            {
                // Top-right quadrant
                return distanceSquared >= radiusSquared && deltaX >= -cornerRadius && deltaY <= cornerRadius;
            }
            else
            {
                // Bottom-right quadrant
                return distanceSquared >= radiusSquared && deltaX >= -cornerRadius && deltaY >= -cornerRadius;
            }
        }


        //private static bool IsPointOnRoundedBoxPerimeter(int x, int y, int startX, int startY, int endX, int endY, int cornerRadius)
        //{
        //    int left = startX + cornerRadius;
        //    int right = endX - cornerRadius;
        //    int top = startY + cornerRadius;
        //    int bottom = endY - cornerRadius;

        //    // Check if point is on the perimeter of the rounded box
        //    //return (x == left && y >= top && y <= bottom) ||
        //    //       (x == right && y >= top && y <= bottom) ||
        //    //       (y == top && x >= left && x <= right) ||
        //    //       (y == bottom && x >= left && x <= right);
        //    //    //||
        //    //    //   IsPointOnRoundedCornerPerimeter(x, y, left, top, cornerRadius) ||
        //    //    //   IsPointOnRoundedCornerPerimeter(x, y, right, top, cornerRadius) ||
        //    //    //   IsPointOnRoundedCornerPerimeter(x, y, left, bottom, cornerRadius) ||
        //    //    //   IsPointOnRoundedCornerPerimeter(x, y, right, bottom, cornerRadius);

        //    return !IsPointOnRoundedCornerPerimeter(x, y, left, top, cornerRadius) ||
        //           !IsPointOnRoundedCornerPerimeter(x, y, right, top, cornerRadius) ||
        //           !IsPointOnRoundedCornerPerimeter(x, y, left, bottom, cornerRadius) ||
        //           !IsPointOnRoundedCornerPerimeter(x, y, right, bottom, cornerRadius);
        //}

        //private static bool IsPointOnRoundedCornerPerimeter(int x, int y, int cornerX, int cornerY, int cornerRadius)
        //{
        //    if (Math.Abs(x - cornerX) <= cornerRadius && Math.Abs(y - cornerY) <= cornerRadius)
        //    {
        //        float distance = (float)Math.Sqrt(Math.Pow(x - cornerX, 2) + Math.Pow(y - cornerY, 2));
        //        return Math.Abs(distance - cornerRadius) < 1f;
        //    }

        //    return false;
        //}

        //private static bool IsPointInsideRoundedBox(int x, int y, int startX, int startY, int endX, int endY, int cornerRadius)
        //{
        //    int left = startX + cornerRadius;
        //    int right = endX - cornerRadius;
        //    int top = startY + cornerRadius;
        //    int bottom = endY - cornerRadius;

        //    // Check if point is inside the rounded box
        //    if (x >= left && x <= right && y >= startY && y <= endY)
        //        return true;
        //    if (x >= startX && x <= endX && y >= top && y <= bottom)
        //        return true;

        //    // Check if point is inside the top-left rounded corner
        //    if (Math.Pow(x - left, 2) + Math.Pow(y - top, 2) <= Math.Pow(cornerRadius, 2))
        //        return true;

        //    // Check if point is inside the top-right rounded corner
        //    if (Math.Pow(x - right, 2) + Math.Pow(y - top, 2) <= Math.Pow(cornerRadius, 2))
        //        return true;

        //    // Check if point is inside the bottom-left rounded corner
        //    if (Math.Pow(x - left, 2) + Math.Pow(y - bottom, 2) <= Math.Pow(cornerRadius, 2))
        //        return true;

        //    // Check if point is inside the bottom-right rounded corner
        //    if (Math.Pow(x - right, 2) + Math.Pow(y - bottom, 2) <= Math.Pow(cornerRadius, 2))
        //        return true;

        //    return false;
        //}

        private static Vector2 RotatePoint(Vector2 point, Vector2 center, float angle)
        {
            float radians = angle.ToRadians();
            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);

            float translatedX = point.X - center.X;
            float translatedY = point.Y - center.Y;

            float rotatedX = (translatedX * cos) - (translatedY * sin);
            float rotatedY = (translatedX * sin) + (translatedY * cos);

            return new Vector2(rotatedX + center.X, rotatedY + center.Y);
        }

        //private static bool IsPointOnRoundedBoxOutline(int x, int y, int startX, int startY, int endX, int endY, int cornerRadius)
        //{
        //    int left = startX + cornerRadius;
        //    int right = endX - cornerRadius;
        //    int top = startY + cornerRadius;
        //    int bottom = endY - cornerRadius;

        //    // Check if point is on the outline of the rounded box
        //    if ((x >= left && x <= right && (y == startY || y == endY)) ||
        //        (y >= top && y <= bottom && (x == startX || x == endX)) ||
        //        IsPointOnRoundedCornerOutline(x, y, left, top, cornerRadius) ||
        //        IsPointOnRoundedCornerOutline(x, y, right, top, cornerRadius) ||
        //        IsPointOnRoundedCornerOutline(x, y, left, bottom, cornerRadius) ||
        //        IsPointOnRoundedCornerOutline(x, y, right, bottom, cornerRadius))
        //    {
        //        return true;
        //    }

        //    return false;
        //}

        //private static bool IsPointOnRoundedCornerOutline(int x, int y, int cornerX, int cornerY, int cornerRadius)
        //{
        //    if (Math.Abs(x - cornerX) <= cornerRadius && Math.Abs(y - cornerY) <= cornerRadius)
        //    {
        //        float distance = (float)Math.Sqrt(Math.Pow(x - cornerX, 2) + Math.Pow(y - cornerY, 2));
        //        return Math.Abs(distance - cornerRadius) < 1f;
        //    }

        //    return false;
        //}




        //public static void DrawRoundedBoxOnImage(Vector2 location, int width, int height, int cornerRadius, float facingAngle, byte[] image, int imageWidth)
        //{
        //    int startX = (int)location.X;
        //    int startY = (int)location.Y;
        //    int endX = startX + width;
        //    int endY = startY + height;

        //    int bytesPerPixel = 4; // RGBA format
        //    int stride = imageWidth * bytesPerPixel;

        //    for (int y = startY; y <= endY; y++)
        //    {
        //        for (int x = startX; x <= endX; x++)
        //        {
        //            if (IsPointInsideRoundedBox(x, y, startX, startY, endX, endY, cornerRadius))
        //            {
        //                int pixelIndex = (y * stride) + (x * bytesPerPixel);
        //                image[pixelIndex] = 0;       // Red component
        //                image[pixelIndex + 1] = 255; // Green component
        //                image[pixelIndex + 2] = 0;   // Blue component
        //                image[pixelIndex + 3] = 255; // Alpha component
        //            }
        //        }
        //    }
        //}

        //private static bool IsPointInsideRoundedBox(int x, int y, int startX, int startY, int endX, int endY, int cornerRadius)
        //{
        //    int left = startX + cornerRadius;
        //    int right = endX - cornerRadius;
        //    int top = startY + cornerRadius;
        //    int bottom = endY - cornerRadius;

        //    // Check if point is inside the rounded box
        //    if (x >= left && x <= right && y >= startY && y <= endY)
        //        return true;
        //    if (x >= startX && x <= endX && y >= top && y <= bottom)
        //        return true;

        //    // Check if point is inside the top-left rounded corner
        //    if (Math.Pow(x - left, 2) + Math.Pow(y - top, 2) <= Math.Pow(cornerRadius, 2))
        //        return true;

        //    // Check if point is inside the top-right rounded corner
        //    if (Math.Pow(x - right, 2) + Math.Pow(y - top, 2) <= Math.Pow(cornerRadius, 2))
        //        return true;

        //    // Check if point is inside the bottom-left rounded corner
        //    if (Math.Pow(x - left, 2) + Math.Pow(y - bottom, 2) <= Math.Pow(cornerRadius, 2))
        //        return true;

        //    // Check if point is inside the bottom-right rounded corner
        //    if (Math.Pow(x - right, 2) + Math.Pow(y - bottom, 2) <= Math.Pow(cornerRadius, 2))
        //        return true;

        //    return false;
        //}

        public static void DrawBoxOnImage(Vector2 location, int width, int height, float facingAngle, byte[] image, int imageWidth)
        {
            int startX = (int)location.X;
            int startY = (int)location.Y;
            int endX = startX + width;
            int endY = startY + height;

            // Calculate center point of the box
            float centerX = startX + (width / 2f);
            float centerY = startY + (height / 2f);

            int bytesPerPixel = 4; // RGBA format
            int stride = imageWidth * bytesPerPixel;

            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    if (x == startX || x == endX - 1 || y == startY || y == endY - 1)
                    {
                        Vector2 rotatedPoint = RotatePoint(new Vector2(x, y), new Vector2(centerX, centerY), facingAngle);
                        int pixelIndex = ((int)rotatedPoint.Y * stride) + ((int)rotatedPoint.X * bytesPerPixel);

                        image[pixelIndex] = 0;       // Red component
                        image[pixelIndex + 1] = 255; // Green component
                        image[pixelIndex + 2] = 0;   // Blue component
                        image[pixelIndex + 3] = 255; // Alpha component
                    }
                }
            }
        }

        private static int GetIndex(int x, int y, int stride, int bytesPerPixel) => (y * stride) + (x * bytesPerPixel);

        private static void SetPixel(byte[] image, int index, Colour colour)
        {
            image[index] = colour.Red;
            image[index + 1] = colour.Green;
            image[index + 2] = colour.Blue;
            image[index + 3] = colour.Alpha;
        }
    }
}
