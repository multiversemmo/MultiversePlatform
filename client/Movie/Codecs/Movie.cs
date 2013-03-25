/********************************************************************

The Multiverse Platform is made available under the MIT License.

Copyright (c) 2012 The Multiverse Foundation

Permission is hereby granted, free of charge, to any person 
obtaining a copy of this software and associated documentation 
files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, 
merge, publish, distribute, sublicense, and/or sell copies 
of the Software, and to permit persons to whom the Software 
is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be 
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE 
OR OTHER DEALINGS IN THE SOFTWARE.

*********************************************************************/

using Axiom.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;

namespace Multiverse.Movie.Codecs
{
    public class Movie : IMovie
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Movie));

        protected int id;
        protected string name, path, textureName;
        protected Texture texture;
        protected Size videoSize, textureSize;
        public enum PLAY_STATE
        {
            INIT,
            BUFFERING,
            RUNNING,
            PAUSED,
            STOPPED,
            UNLOADED,
        }
        protected PLAY_STATE ps;
        protected string altImage;
        protected bool altImageShown;

        public Movie()
        {
            id = Manager.GetNewIdentifier();
            name = null;
            path = null;
            textureName = null;
            texture = null;
            videoSize = Size.Empty;
            textureSize = Size.Empty;
            ps = PLAY_STATE.INIT;
            altImage = null;
            altImageShown = false;
        }

        ~Movie()
        {
            Unload();
        }

        #region IMovie methods
        public virtual string CodecName()
        {
            // override me
            return null;
        }
        public virtual string Name()
        {
            return name;
        }
        public virtual string Path()
        {
            return path;
        }
        public virtual string TextureName()
        {
            return textureName;
        }
        public virtual int ID()
        {
            return id;
        }
        public virtual Size VideoSize()
        {
            return videoSize;
        }
        public virtual Size TextureSize()
        {
            return textureSize;
        }
        public virtual Texture Texture()
        {
            return texture;
        }
        public virtual string AltImage()
        {
            return altImage;
        }
        public virtual void SetAltImage(string image)
        {
            altImage = image;
        }
        public virtual bool ShowAltImage()
        {
            bool ans = false;
            if (!altImageShown && (altImage != null))
            {
                Manager.ShowAltImage(TextureName(), altImage);
                ans = true;
            }
            altImageShown = true;
            return ans;
        }
        public virtual bool HideAltImage()
        {
            bool ans = false;
            if (altImageShown)
            {
                Manager.HideAltImage(TextureName());
                ans = true;
            }
            altImageShown = false;
            return ans;
        }
        public virtual bool ReplaceWorldObject(string name)
        {
            return Manager.ReplaceWorldObject(this, name);
        }
        public virtual bool SetTextureCoordinates(string material)
        {
            return Manager.SetTextureCoordinates(this, material);
        }
        public virtual bool Play()
        {
            ps = PLAY_STATE.RUNNING;
            return true;
        }
        public virtual bool Pause()
        {
            ps = PLAY_STATE.PAUSED;
            return true;
        }
        public virtual bool Stop()
        {
            ps = PLAY_STATE.STOPPED;
            return true;
        }
        public virtual void Unload()
        {
            ps = PLAY_STATE.UNLOADED;
            return;
        }
        public virtual bool SetParameter(string name, string val)
        {
            ICodec codec = Manager.Instance.FindCodec(CodecName());
            if ((codec != null) && (!codec.ValidateParameter(name, val)))
            {
                log.ErrorFormat("Parameter '{0}' is not valid with value '{1}' (did you override ICodec.ValidateParameter()?)",
                                name, val);
            }
            return false;
        }
        #endregion
    }
}
