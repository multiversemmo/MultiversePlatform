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
using System.Text;

namespace Multiverse.Movie.Codecs
{
    public class Codec : ICodec
    {
        protected IDictionary MovieList;

        #region ICodec methods
        public virtual void Start()
        {
            MovieList = new Hashtable();
        }

        public virtual void Stop()
        {
            UnloadAll();
            MovieList = null;
        }

        public virtual string Name()
        {
            // tell the manager not to instantiate this base class
            return null;
        }

        public virtual bool ValidateParameter(string name, string val)
        {
            // override me
            return false;
        }

        public virtual IMovieTexture CreateMovieTexture(string name)
        {
            // override me
            return null;
        }

        public virtual IMovie LoadFile(string name, string file)
        {
            return LoadFile(name, file, null);
        }

        public virtual IMovie LoadFile(string name, string file, string textureName)
        {
            // override me
            return null;
        }

        public virtual IMovie LoadStream(string name, string url)
        {
            return LoadStream(name, url, null);
        }

        public virtual IMovie LoadStream(string name, string url, string textureName)
        {
            // override me
            return null;
        }

        public virtual IMovie FindMovie(string name)
        {
            IDictionaryEnumerator ie = MovieList.GetEnumerator();
            while (ie.MoveNext())
            {
                IMovie movie = (ie.Value as IMovie);
                if (movie.Name() == name)
                {
                    return movie;
                }
            }
            return null;
        }

        public virtual void UnloadAll()
        {
            IDictionaryEnumerator ie = MovieList.GetEnumerator();
            while(ie.MoveNext())
            {
                IMovie im = ie.Value as IMovie;
                if (im != null)
                {
                    im.Stop();
                    im.Unload();
                }
            }
            MovieList.Clear();
        }

        public virtual bool UnloadMovie(IMovie im)
        {
            if (HasMovie(im))
            {
                im.Stop();
                im.Unload();
                RemoveMovie(im);
                return true;
            }
            return false; 
        }
        #endregion

        public virtual void AddMovie(IMovie m)
        {
            MovieList[m.ID()] = m;
        }

        public virtual bool HasMovie(IMovie m)
        {
            return (m != null) && (MovieList[m.ID()] != null);
        }

        public virtual void RemoveMovie(IMovie m)
        {
            if ((m != null) && (MovieList[m.ID()] != null))
            {
                MovieList.Remove(m.ID());
            }
        }
    }
}
