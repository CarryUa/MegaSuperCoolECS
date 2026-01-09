#ifndef PNG_DECODER_H
#define PNG_DECODER_H
#include <vector>

enum ColorType : unsigned char
{
    Grayscale = 0,
    Truecolor = 2,
    Indexed = 3,
    GrayscaleAlpha = 4,
    TruecolorAlpha = 6
};

#pragma pack(push, 1)
struct PNGImage
{
    unsigned int Width;
    unsigned int Height;
    unsigned char Bit_depth;
    unsigned char Bytes_per_pixel;
    ColorType Color_type;
    unsigned char *Pixel_data;
    size_t Pixel_data_len = 0;
};
#pragma pack(pop)
extern "C"
{
    __declspec(dllexport) PNGImage read_image(const char *filename);
    __declspec(dllexport) void create_image_object(PNGImage *output);
}

#endif