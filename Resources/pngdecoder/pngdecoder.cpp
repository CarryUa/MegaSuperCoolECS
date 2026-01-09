#include <pngdecoder.h>
#include <iostream>
#include <fstream>
#include <cmath>
#include <zlib.h>
#include <algorithm>

using namespace std;

struct PNGChunk
{
    PNGChunk() {}
    PNGChunk(char *l, char *t, char *d, char *c)
    {
        memcpy(length, l, 4);
        memcpy(type, t, 4);
        data = (unsigned char *)d;
        for (size_t i = 0; i < 4; i++)
            data_len = (data_len << 8) | (unsigned char)l[i];
        memcpy(crc, c, 4);
    }

    unsigned char length[4];
    unsigned int data_len;
    unsigned char type[4];
    unsigned char *data;
    unsigned char crc[4];
};

bool try_apply_decompression(PNGImage &image)
{
    unsigned char samples_per_pixel;

    switch (image.Color_type)
    {
    case ColorType::Grayscale:
        samples_per_pixel = 1;
        break;
    case ColorType::Truecolor:
        samples_per_pixel = 3;
        break;
    case ColorType::Indexed:
        samples_per_pixel = 1;
        break;
    case ColorType::GrayscaleAlpha:
        samples_per_pixel = 2;
        break;
    case ColorType::TruecolorAlpha:
        samples_per_pixel = 4;
        break;
    }

    image.Bytes_per_pixel = (samples_per_pixel * image.Bit_depth + 7) / 8;

    unsigned long decompr_len = (image.Width * image.Bytes_per_pixel + 1) * image.Height;

    unsigned char *dest = new unsigned char[decompr_len];

    int result = uncompress(dest, &decompr_len, image.Pixel_data, image.Pixel_data_len);

    if (result != Z_OK)
    {
        cerr << "Error decompressing file. " << result << endl;
        return false;
    }

    delete[] image.Pixel_data;

    image.Pixel_data_len = decompr_len;

    image.Pixel_data = new unsigned char[image.Pixel_data_len];

    memcpy(image.Pixel_data, dest, image.Pixel_data_len);

    delete[] dest;
    return true;
}

unsigned char get_paeth_predictor(unsigned char a, unsigned char b, unsigned char c)
{
    int p = a + b - c;

    unsigned char pa = abs(p - a), pb = abs(p - b), pc = abs(p - c);

    return min(pa, min(pb, pc));
}

bool try_apply_reverse_filtering(PNGImage &image)
{
    // 1(filter byte) + width * bpp. In bytes
    size_t src_row_size = 1 + image.Width * image.Bytes_per_pixel;

    size_t out_len = (src_row_size - 1) * image.Height;
    // char array of reverse filtered pixel data. Size = (src_row_size - 1 filter byte) * height
    unsigned char *out = new unsigned char[out_len];

    // Iterate over each row
    for (unsigned int y = 0; y < image.Height; y++)
    {
        // The row start position considering filter byte.
        size_t row_start_pos_w_fb = src_row_size * y;

        // The row start position NOT considering filter byte.
        size_t row_start_pos = y * (src_row_size - 1);

        unsigned char filter_byte = image.Pixel_data[src_row_size * y];

        switch (filter_byte)
        {
        case 0: // NONE
        {
            // Append the out(without filter byte)
            for (int x = 1; x < src_row_size; x++)
                out[row_start_pos + (x - 1)] = image.Pixel_data[row_start_pos_w_fb + x];

            break;
        }

        case 1: // SUB (to the left)
        {
            for (int x = 1; x < src_row_size; x++)
            {
                unsigned char A = 0;
                if (x - 1 >= image.Bytes_per_pixel)
                    A = out[row_start_pos + (x - 1) - image.Bytes_per_pixel];

                // Sub(x) = Raw(x) - Raw(x - bpp)
                out[row_start_pos + (x - 1)] = image.Pixel_data[row_start_pos_w_fb + x] + A;
            }

            break;
        }

        case 2: // UP (above)
        {
            for (int x = 1; x < src_row_size; x++)
            {
                unsigned char B = 0;
                if (y > 0)
                    B = out[(y - 1) * (src_row_size - 1) + x - 1];

                out[row_start_pos + (x - 1)] = image.Pixel_data[row_start_pos_w_fb + x] + B;
            }

            break;
        }

        case 3: // AVG
        {

            for (int x = 1; x < src_row_size; x++)
            {
                // left
                unsigned char A = 0;
                if (x - 1 >= image.Bytes_per_pixel)
                    A = out[row_start_pos + (x - 1) - image.Bytes_per_pixel];

                // above
                unsigned char B = 0;
                if (y > 0)
                    B = out[(y - 1) * (src_row_size - 1) + x - 1];

                out[row_start_pos + (x - 1)] = image.Pixel_data[row_start_pos_w_fb + x] + (A + B) / 2;
            }

            break;
        }

        case 4: // PAETH
        {

            for (int x = 1; x < src_row_size; x++)
            {
                bool is_first_x = x - 1 >= image.Bytes_per_pixel;
                bool is_first_y = y > 0;

                // left
                unsigned char A = 0;
                if (is_first_x)
                    A = out[row_start_pos + (x - 1) - image.Bytes_per_pixel];

                // above
                unsigned char B = 0;
                if (is_first_y)
                    B = out[(y - 1) * (src_row_size - 1) + x - 1];

                // left-above
                unsigned char C = 0;
                if (is_first_x && is_first_y)
                    C = out[(y - 1) * (src_row_size - 1) + x - 1 - image.Bytes_per_pixel];

                unsigned char paeth_predictor = get_paeth_predictor(A, B, C);

                out[row_start_pos + (x - 1)] = image.Pixel_data[row_start_pos_w_fb + x] + paeth_predictor;
            }
        }
        }
    }

    delete[] image.Pixel_data;
    image.Pixel_data = new unsigned char[out_len];

    memcpy(image.Pixel_data, out, out_len);
    image.Pixel_data_len = out_len;

    delete[] out;

    return true;
}
unsigned int get_ihdr_w(const PNGChunk &ihdr)
{
    unsigned int w = 0;
    for (int i = 0; i < 4; i++) // width data starts at byte 0
    {
        w = (w << 8) | ihdr.data[i];
    }
    return w;
}

unsigned int get_ihdr_h(const PNGChunk &ihdr)
{
    unsigned int h = 0;
    for (int i = 4; i < 8; i++) // height data starts at byte 5
    {
        h = (h << 8) | ihdr.data[i];
    }
    return h;
}

unsigned char get_ihdr_bit_depth(const PNGChunk &ihdr)
{
    return ihdr.data[8]; // bit depth is at byte 9
}

unsigned char get_ihdr_color_type(const PNGChunk &ihdr)
{
    return ihdr.data[9]; // color type is at byte 10
}
unsigned char get_ihdr_compresion_method(const PNGChunk &ihdr)
{
    return ihdr.data[10]; // color type is at byte 11
}
unsigned char get_ihdr_filter_method(const PNGChunk &ihdr)
{
    return ihdr.data[11]; // color type is at byte 12
}
unsigned char get_ihdr_interlace_method(const PNGChunk &ihdr)
{
    return ihdr.data[12]; // color type is at byte 13
}

bool is_png(const char *filename)
{

    ifstream f(filename, ios::binary);
    if (!f)
    {
        cerr << "Failed to open file" << endl;
        return false;
    }

    const unsigned char valid_signature[8] = {0x89, 'P', 'N', 'G', 0x0D, 0x0A, 0x1A, 0x0A};
    char signature[8];

    f.read(signature, 8);
    f.close();

    return memcmp(signature, valid_signature, 8) == 0;
}

PNGImage read_image(const char *filename)
{
    ifstream f = ifstream(filename, ios::binary);
    f.seekg(8); // skip signature

    size_t total_idat_size = 0;

    while (!f.eof())
    {
        char *length = new char[4];
        f.read(length, 4);
        char *type = new char[4];
        f.read(type, 4);

        unsigned int data_len = 0;
        for (int i = 0; i < 4; i++)
            data_len = (data_len << 8) | (unsigned char)length[i];

        delete[] length;

        f.seekg(data_len + 4, ios::cur);

        if (memcmp(type, "IDAT", 4) == 0)
            total_idat_size += data_len;

        else if (memcmp(type, "IEND", 4) == 0)
        {
            delete[] type;
            break;
        }
        delete[] type;
    }

    PNGImage image = PNGImage();
    unsigned char *idat_contents = new unsigned char[total_idat_size];
    size_t last_idat_index = 0;

    f.seekg(8);

    while (!f.eof())
    {
        if (!f)
            break;

        // read length
        char *length = new char[4];
        f.read(length, 4);

        // read name(type)
        char *type = new char[4];
        f.read(type, 4);

        // interpret length as uint
        unsigned int *data_len = new unsigned int(0);
        for (int i = 0; i < 4; i++)
            *data_len = (*data_len << 8) | (unsigned char)length[i];

        // read data
        char *data = new char[*data_len];
        f.read(data, *data_len);

        // read crc
        char *crc = new char[4];
        f.read(crc, 4);

        if (memcmp(type, "IEND", 4) == 0)
        {
            delete[] data;
            delete[] type;
            delete[] crc;
            delete data_len;
            delete[] length;
            break;
        }

        else if (memcmp(type, "IHDR", 4) == 0)
        {
            PNGChunk IHDR = PNGChunk(length, type, data, crc);
            image.Width = get_ihdr_w(IHDR);
            image.Height = get_ihdr_h(IHDR);
            image.Bit_depth = get_ihdr_bit_depth(IHDR);
            image.Color_type = static_cast<ColorType>(get_ihdr_color_type(IHDR));
            delete[] data;
            delete[] type;
            delete[] crc;
            delete data_len;
            delete[] length;
        }

        else if (memcmp(type, "IDAT", 4) == 0)
        {
            if (*data_len > 0)
                for (size_t i = 0; i < *data_len; i++)
                {
                    idat_contents[last_idat_index] = data[i];
                    last_idat_index++;
                }

            delete[] data;
            delete[] type;
            delete[] crc;
            delete data_len;
            delete[] length;
        }

        else
        {
            delete[] data;
            delete[] type;
            delete[] crc;
            delete data_len;
            delete[] length;
        }

        if (f.eof())
        {
            cerr << "Corrupted PNG file: unexpected end of file before IEND chunk" << endl;
            break;
        }
    }
    image.Pixel_data_len = last_idat_index;
    image.Pixel_data = new unsigned char[image.Pixel_data_len];

    memcpy(image.Pixel_data, idat_contents, image.Pixel_data_len);
    delete[] idat_contents;

    f.close();

    if (!try_apply_decompression(image))
    {
        delete[] image.Pixel_data;
        return PNGImage();
    }

    if (!try_apply_reverse_filtering(image))
    {
        delete[] image.Pixel_data;
        return PNGImage();
    }

    return image;
}

int main(int argc, const char *argv[])
{
    if (argc < 2)
        return 0;

    PNGImage image = read_image(argv[1]);

    size_t index = 0;
    for (int i = 0; i < image.Height; i++)
    {
        for (long j = 0; j < image.Width * image.Bytes_per_pixel; j++)
        {
            cout << hex << (int)image.Pixel_data[index] << " ";
            index++;
        }
        cout << endl;
    }
}