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

package multiverse.server.util;

import java.lang.reflect.Field;

public class ServerVersion {
    public static final String ServerMajorVersion = "1.5";

    public static String getVersionString()
    {
        return ServerMajorVersion + " " + getBuildNumber() + " (" +
                getBuildString() + " " + getBuildDate() + ")";
    }

    public static String getBuildString()
    {
        if (buildString != null)
            return buildString;

        return getFieldValue("buildString","-");
    }

    public static String getBuildDate()
    {
        if (buildDate != null)
            return buildDate;

        return getFieldValue("buildDate","-");
    }

    public static String getBuildNumber()
    {
        if (buildNumber != null)
            return buildNumber;

        return getFieldValue("buildNumber","0");
    }

    public static final int VERSION_LESSER = -1;
    public static final int VERSION_EQUAL = 0;
    public static final int VERSION_GREATER = 1;
    public static final int VERSION_FORMAT_ERROR = -9;

    public static int compareVersionStrings(String leftVersion,
        String rightVersion)
    {
        float left = extractVersion(leftVersion);
        float right = extractVersion(rightVersion);
        if (left == 0.0 || right == 0.0) {
            return VERSION_FORMAT_ERROR;
        }
        if (left == right)
            return VERSION_EQUAL;
        if (left < right)
            return VERSION_LESSER;
        if (left > right)
            return VERSION_GREATER;
        return VERSION_FORMAT_ERROR;
    }

    public static float extractVersion(String versionString)
    {
        int ii=0;
        for ( ; ii < versionString.length(); ii++) {
            char c = versionString.charAt(ii);
            if (! Character.isDigit(c)) {
                break;
            }
        }

        if (ii == 0) { return 0.0F; }
        if (ii == versionString.length()) { return 0.0F; }
        if (versionString.charAt(ii) != '.') { return 0.0F; }
        ii++;

        for ( ; ii < versionString.length(); ii++) {
            char c = versionString.charAt(ii);
            if (! Character.isDigit(c)) {
                break;
            }
        }

        String versionNumber = versionString.substring(0,ii);

        float num = Float.parseFloat(versionNumber);
        return num;
    }

    private static String getFieldValue(String fieldName,
            String defaultValue)
    {
        try {
            Class buildInfo = Class.forName("multiverse.server.util.BuildInfo");
            Field stringField = buildInfo.getField(fieldName);
            return (String) stringField.get(null);
        }
        catch (IllegalAccessException ex) {
        }
        catch (NoSuchFieldException ex) {
        }
        catch (ClassNotFoundException ex) {
        }

        return defaultValue;
    }

    private static String buildString = null;
    private static String buildDate = null;
    private static String buildNumber = null;
}
