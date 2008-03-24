using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Diagnostics;

namespace Svg
{
    /// <summary>
    /// The class that all SVG elements should derive from when they are to be rendered.
    /// </summary>
    public abstract partial class SvgGraphicsElement : SvgElement, ISvgStylable
    {
        private bool _dirty;
        private bool _requiresSmoothRendering;

        /// <summary>
        /// Gets the <see cref="GraphicsPath"/> for this element.
        /// </summary>
        public abstract GraphicsPath Path { get; }
        /// <summary>
        /// Gets the bounds of the element.
        /// </summary>
        /// <value>The bounds.</value>
        public abstract RectangleF Bounds { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this element's <see cref="Path"/> is dirty.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the path is dirty; otherwise, <c>false</c>.
        /// </value>
        protected virtual bool IsPathDirty
        {
            get { return this._dirty; }
            set { this._dirty = value; }
        }

        /// <summary>
        /// Gets or sets a value to determine if anti-aliasing should occur when the element is being rendered.
        /// </summary>
        protected virtual bool RequiresSmoothRendering
        {
            get { return this._requiresSmoothRendering; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SvgGraphicsElement"/> class.
        /// </summary>
        public SvgGraphicsElement()
        {
            this._dirty = true;
            this._requiresSmoothRendering = false;
        }

        /// <summary>
        /// Renders the <see cref="SvgElement"/> and contents to the specified <see cref="Graphics"/> object.
        /// </summary>
        /// <param name="graphics">The <see cref="Graphics"/> object to render to.</param>
        protected override void Render(Graphics graphics)
        {
            if (this.Path != null && this.Visible)
            {
                this.PushTransforms(graphics);

                // If this element needs smoothing enabled turn anti aliasing on
                if (this.RequiresSmoothRendering)
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                }

                // Fill first so that the stroke can overlay
                if (!SvgPaintServer.IsNullOrEmpty(this.Fill))
                {
                    using (Brush brush = this.Fill.GetBrush(this, this.FillOpacity))
                    {
                        if (brush != null)
                        {
                            graphics.FillPath(brush, this.Path);
                        }
                    }
                }

                // Stroke is the last thing to do
                if (!SvgPaintServer.IsNullOrEmpty(this.Stroke))
                {
                    float strokeWidth = this.StrokeWidth.ToDeviceValue(this);
                    using (Pen pen = new Pen(this.Stroke.GetBrush(this, this.StrokeOpacity), strokeWidth))
                    {
                        if (pen != null)
                        {
                            graphics.DrawPath(pen, this.Path);
                        }
                    }
                }

                // Reset the smoothing mode
                if (this.RequiresSmoothRendering && graphics.SmoothingMode == SmoothingMode.AntiAlias)
                {
                    graphics.SmoothingMode = SmoothingMode.Default;
                }

                this.PopTransforms(graphics);
            }
        }
    }
}