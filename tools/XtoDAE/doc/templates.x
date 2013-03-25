template Header {
    <3D82AB43-62DA-11cf-AB39-0020AF71E433>
    WORD major;
    WORD minor;
    DWORD flags;
}
template Vector {
    <3D82AB5E-62DA-11cf-AB39-0020AF71E433>
    FLOAT x;
    FLOAT y;
    FLOAT z;
}
template ColorRGB {
    <D3E16E81-7835-11cf-8F52-0040333594A3>
    FLOAT red;
    FLOAT green;
    FLOAT blue;
}
template ColorRGBA {
    <35FF44E0-6C7C-11cf-8F52-0040333594A3>
    FLOAT red;
    FLOAT green;
    FLOAT blue;
    FLOAT alpha;
}
template Coords2d {
    <F6F23F44-7686-11cf-8F52-0040333594A3>
    FLOAT u;
    FLOAT v;
}
template Matrix4x4 {
    <F6F23F45-7686-11cf-8F52-0040333594A3>
    array FLOAT matrix[16];
}
template FrameTransformMatrix {
    <F6F23F41-7686-11cf-8F52-0040333594A3>
    Matrix4x4 frameMatrix;
}
template Frame {
    <3D82AB46-62DA-11cf-AB39-0020AF71E433>
    [ ... ]
}
template MeshFace {
    <3D82AB5F-62DA-11cf-AB39-0020AF71E433>
    DWORD nFaceVertexIndices;
    array DWORD faceVertexIndices[nFaceVertexIndices];
}
template Mesh {
    <3D82AB44-62DA-11cf-AB39-0020AF71E433>
    DWORD nVertices;
    array Vector vertices[nVertices];
    DWORD nFaces;
    array MeshFace faces[nFaces];
    [ ... ]                // An open template
}
template TextureFilename {
    <A42790E1-7810-11cf-8F52-0040333594A3>
    STRING filename;
}
template Material {
    <3D82AB4D-62DA-11cf-AB39-0020AF71E433>
    ColorRGBA faceColor;
    FLOAT power;
    ColorRGB specularColor;
    ColorRGB emissiveColor;
    [ ... ]
}
template MeshTextureCoords {
    <F6F23F40-7686-11cf-8F52-0040333594A3>
    DWORD nTextureCoords;
    array Coords2d textureCoords[nTextureCoords]
}
template MeshNormals {
    <F6F23F43-7686-11cf-8F52-0040333594A3>
    DWORD nNormals;
    array Vector normals[nNormals];
    DWORD nFaceNormals;
    array MeshFace faceNormals[nFaceNormals];
}
template MeshMaterialList {
    <F6F23F42-7686-11cf-8F52-0040333594A3>
    DWORD nMaterials;
    DWORD nFaceIndexes;
    array DWORD FaceIndexes[nFaceIndexes];
    [ Material ]
}
