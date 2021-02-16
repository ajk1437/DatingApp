export interface Group {
    name: string;
    connections: Conection[];
}

interface Conection {
    connectionId: string;
    username: string;   
}