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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;

using Axiom.Core;
using Axiom.RenderSystems.DirectX9;

using Microsoft.DirectX;
using Microsoft.DirectX.PrivateImplementationDetails;
using Microsoft.DirectX.Direct3D;
using D3D = Microsoft.DirectX.Direct3D;

namespace Multiverse.Movie
{
    public class D3DMovieTexture : D3DTexture, IMovieTexture
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(D3DMovieTexture));

        private ICodec codec;
        private IMovie movie;
        private List<string> materialnames;
        private string moviename;
        private string codecname;
        private string path;
        private string alt;
        private IDictionary parameters;

        private int loadcount;
        private bool stopload;

        public D3DMovieTexture(string texturename)
            : base(texturename, true,
            (TextureManager.Instance as D3DTextureManager).Device)
        {
            materialnames = new List<string>();
            codec = null;
            movie = null;
            parameters = new Hashtable();
            codecname = null;
            moviename = null;
            path = null;
            alt = null;
            loadcount = 0;
            stopload = false;
            if (TextureManager.Instance.GetByName(texturename) != null)
            {
                log.ErrorFormat("MovieD3DTexture: A texture already exists in the system named {0}, not replacing it.", 
                                texturename);
            }
            else
            {
                TextureManager.Instance.Add(this);
            }
        }

        public void AddMaterial(string materialname)
        {
            if ((materialname != null) && (!materialnames.Contains(materialname)))
            {
                materialnames.Add(materialname);
            }
        }

        #region IMovieTexture methods
        public void SetParameter(string name, string val)
        {
            switch (name)
            {
                case MovieTextureSource.MV_CODEC_NAME:
                    codecname = val;
                    codec = Manager.Instance.FindCodec(codecname);
                    if (codec == null)
                    {
                        log.ErrorFormat("Could not find codec '{0}'", codecname);
                    }
                    break;
                case MovieTextureSource.MV_MOVIE_NAME:
                    moviename = val;
                    break;
                case MovieTextureSource.MV_PATH_NAME:
                    path = val;
                    break;
                case MovieTextureSource.MV_ALT_NAME:
                    alt = val;
                    break;
                default:
                    parameters.Add(name, val);
                    break;
            }
        }
        #endregion

        #region D3DTexture methods
        public override void Load()
        {
            if ((movie == null) && (!stopload))
            {
                if (path == null)
                {
                    if (alt == null)
                    {
                        log.Warn("MovieD3DTexture: No filename, not loading movie texture");
                    }
                    stopload = true;
                }
                else
                {
                    if (codec == null)
                    {
                        log.ErrorFormat("MovieD3DTexture: Could not find codec ({0})", codecname);
                        stopload = true;
                    }
                    else
                    {
                        IMovie m = codec.LoadFile(moviename, path, Name);
                        if (m == null)
                        {
                            log.ErrorFormat("MovieD3DTexture: Movie '{0}' failed to load", path);
                            stopload = true;
                        }
                        else
                        {
                            if (alt != null)
                            {
                                m.SetAltImage(alt);
                            }
                            IDictionaryEnumerator ie = parameters.GetEnumerator();
                            while (ie.MoveNext())
                            {
                                string name = (string)ie.Key;
                                string val = (string)ie.Value;
                                if (!codec.ValidateParameter(name, val))
                                {
                                    log.WarnFormat("MovieD3DTexture got unknown param: {0} ({1})", name, val);
                                }
                                else
                                {
                                    if (!m.SetParameter(name, val))
                                    {
                                        log.ErrorFormat("Failed to set parameter on movie '{0}' (tried to set '{1}')", name, val);
                                        log.Error("Continuing anyway");
                                    }
                                }
                            }
                            movie = m;
                            foreach(string material in materialnames)
                            {
                                Manager.SetTextureCoordinates(movie, material);
                            }
                        }
                    }
                }
                if (stopload && (alt != null))
                {
                    log.InfoFormat("MovieD3DTexture: Load alt image '{0}'", alt);
                    Manager.ShowAltImage(moviename, alt);
                }
            }
            if (movie != null)
            {
                loadcount++;
                base.Load();
            }
        }
        public override void Unload()
        {
            loadcount--;
            if (loadcount == 0)
            {
                if (movie != null)
                {
                    ICodec c = Manager.Instance.FindCodec(movie.CodecName());
                    if (c != null)
                    {
                        c.UnloadMovie(movie);
                        movie = null;
                    }
                }
            }
            base.Unload();
        }
        #endregion
    }
}
