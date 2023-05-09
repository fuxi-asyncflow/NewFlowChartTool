from binary_reader import BinaryReader
import uuid
from typing import Dict

class Flag:
    NodeData = 1
    VariableData = 2
    EventData = 3
    Frame = 16
    GraphDebugData = 17


class GraphDebugInfo:
    def __init__(self):
        self.node_map = {}
        self.node_uid_map = {}
        self.var_map = {}


    def deserialize(self, br: BinaryReader):
        count = br.read_int32()
        for i in range(count):
            uid = br.read_uuid()
            id = br.read_uint16()
            self.node_uid_map[id] = uid
            self.node_map[uid] = id

        count = br.read_int32()
        for i in range(count):
            var_name = br.read_str()
            var_id = br.read_uint16()
            self.var_map[var_id] = var_name

    def get_node_uid(self, node_id):
        return self.node_uid_map[node_id]
    
    def get_var_name(self, var_id):
        return self.var_map[var_id]


class GraphInfo:
    def __init__(self):
        self.agent_Id = 0
        self.owner_node_addr = 0
        self.owner_node_id = 0
        self.owner_node_uuid = ''
        self.object_name = ''
        self.graph_name = ''
        self.owner_graph_name = ''
        self.graph_uid : uuid.UUID = None
        self.host = ''
        self.port = 0


    def deserialize(self, br: BinaryReader):
        self.agent_Id = br.read_uint64()
        self.owner_node_addr = br.read_uint64()
        self.owner_node_id = br.read_int32()
        self.owner_node_uuid = br.read_str()
        self.object_name = br.read_str()
        self.graph_name = br.read_str()
        self.owner_graph_name = br.read_str()
        self.graph_uid = uuid.UUID(br.read_str())
        self.host = br.read_str()
        self.port = br.read_int32()

class NodeStatusData:
    def __init__(self, gi: GraphDebugInfo):
        self.graph_debug_info = gi
        self.node_id = 0
        self.node_uid = None
        self.old_status = 0
        self.new_status = 0
        self.result = False

    def deserialize(self, br: BinaryReader):
        self.node_id = br.read_uint16()
        print(self.node_id)
        self.old_status = br.read_int8()
        self.new_status = br.read_int8()
        self.result = br.read_int8() != 0
        self.node_uid = self.graph_debug_info.get_node_uid(self.node_id)


class VariablesStatusData:
    def __init__(self, gi: GraphDebugInfo):
        self.graph_debug_info = gi
        self.var_name = ''
        self.node_uid = ''
        self.old_value = ''
        self.new_value = ''

    def deserialize(self, br: BinaryReader):
        self.var_name = self.graph_debug_info.get_var_name(br.read_int8())        
        self.old_value = br.read_str()
        self.new_value = br.read_str()
        self.node_uid = self.graph_debug_info.get_node_uid(br.read_uint16())

class GraphDebugData:
    def __init__(self, graph_debug_info_dict: Dict[str, GraphDebugInfo], graph_info_dict: Dict[uuid.UUID, GraphInfo]):
        self.graph_name = ''
        self.graph_uid = None
        self.frame = 0
        self.time = 0
        self.data = []
        self.graph_debug_info_dict = graph_debug_info_dict
        self.graph_info_dict = graph_info_dict

    def deserialize(self, br: BinaryReader):
        br.read_int32()
        self.graph_uid = br.read_uuid()
        gi = self.graph_info_dict[self.graph_uid]        
        count = br.read_int32()
        for i in range(count):
            flag = br.read_int8()
            if flag == Flag.NodeData:
                print(self.graph_info_dict)
                gi = self.graph_info_dict[gi.graph_uid]
                node_data = NodeStatusData(self.graph_debug_info_dict[gi.graph_name])
                node_data.deserialize(br)
                self.data.append(node_data)
                print(node_data)
            elif flag == Flag.VariableData:
                var_data = VariablesStatusData()
                var_data.deserialize(br)
                print(var_data)
                self.data.append(var_data)

class ReplayFile:
    def __init__(self):
        pass    

    def load(self, file_path: str):
        graph_debug_info_dict : Dict[str, GraphDebugInfo] = {}
        graph_info_dict : Dict[uuid.UUID, GraphInfo] = {}
        with open(file_path, "rb") as f:
            data = f.read()
        f.close()
        
        reader = BinaryReader()
        reader.set_bytes(data)

        
        # read static info
        header_offset = reader.read_int32()
        
        count = reader.read_int32()
        for i in range(count):
            graph_name = reader.read_str()
            
            graph_debug_info = GraphDebugInfo()
            graph_debug_info.deserialize(reader)
            graph_debug_info_dict[graph_name] = graph_debug_info
            print(graph_name, graph_debug_info)

        count = reader.read_int32()
        for i in range(count):
            uid = reader.read_uuid()
            graph_info = GraphInfo()
            graph_info.deserialize(reader)
            graph_info_dict[uid] = graph_info
            print(uid, graph_info)

        while True:
            try:
                flag = reader.read_int8()
            except Exception as e:
                print(e)
                break

            if flag == Flag.Frame:
                frame = reader.read_uint64()
                time = reader.read_uint64()
                print(f'frame {frame} time {time}')
            elif flag == Flag.GraphDebugData:
                gdd = GraphDebugData(graph_debug_info_dict, graph_info_dict)
                gdd.deserialize(reader)
                print(len(gdd.data))