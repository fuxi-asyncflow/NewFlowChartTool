import struct
import io
import uuid

class BinaryReader:
    def __init__(self):
        self.stream = None

    def set_bytes(self, data: bytes):
        self.stream = io.BytesIO(data)

    def _unpack(self, fmt):
        size = struct.calcsize(fmt)
        buf = self.stream.read(size)
        return struct.unpack(fmt, buf)

    def read_int32(self):
        return struct.unpack('i', self.stream.read(4))[0]
    
    def read_int8(self):
        return struct.unpack('b', self.stream.read(1))[0]
    
    def read_int16(self):
        return struct.unpack('h', self.stream.read(2))[0]
    
    def read_uint16(self):
        return struct.unpack('H', self.stream.read(2))[0]
    
    def read_uint8(self):
        return struct.unpack('B', self.stream.read(1))[0]
    
    def read_uuid(self) -> uuid.UUID:
        return uuid.UUID(bytes_le = self.stream.read(16))
    
    def read_uint64(self):
        return struct.unpack('Q', self.stream.read(8))[0]

    
    def read_7bint(self):
        v = 0
        shift = 0
        while True:
            b = self.read_uint8()
            if b > 127:
                v = v + ((b - 128) << shift)
                shift = shift + 7
            else:
                v = v + (b << shift)
                break
        return v
    
    def read_str(self) -> str:
        size = self.read_7bint()
        #bytes = struct.unpack(f'{size}s', self.stream.read(size))[0]
        return self.stream.read(size).decode()

if __name__ == '__main__':
    data = b'\xa3\x02'
    br = BinaryReader()
    br.set_bytes(data)
    #print(br.read_7bint())
    assert(br.read_7bint() == 0x123)

