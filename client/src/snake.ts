import {HubConnection} from "@microsoft/signalr";

export const canvas = document.getElementById("canvas") as HTMLCanvasElement;
canvas.height = 700;
canvas.width = 1100;
export const ctx = canvas.getContext('2d');

interface IPixel {
    x: number;
    y: number;
}

function Trail(x: number, y: number) {
    this.x = x;
    this.y = y;
}

const initialTail = 10;

interface ISnake {
    id: string;
    x: number;
    y: number;
    tail: number;
    trail: Array<{ x: number, y: number }>;
    description: string;
}

export class Snake implements ISnake {
    id: ISnake["id"];
    x: ISnake["x"];
    y: ISnake["y"];
    tail: ISnake["tail"];
    trail: ISnake["trail"];
    description: ISnake["description"];
    direction: "xx" | "yy" | "-yy" | "-xx";
    speed; //todo calculate the speed according the trail size

    connection: HubConnection;

    constructor(props: {id: string, description: string, x?: number, y?: number, direction?, speed?, trail?, tail?}, connection) {
        const {description,id, direction, speed, tail, trail, x, y} = props;
        this.id = id;
        this.x = x || canvas.width / 2;
        this.y = y || canvas.height / 2;
        this.description = description;
        this.direction = "xx" || direction;
        this.speed = 5 || speed;
        this.trail = [] || trail;
        this.tail = initialTail || tail;
        this.connection = connection;
    }

    render = (isMain) => {
        this.trail.forEach(i => renderPixel(i.x, i.y, 5, 5, isMain ? 'red' : 'green'));

        if(this.tail > (initialTail * 10)){
            this.speed = 1
        }
    };
}

export function renderPixel(x, y, w, h, color){
    ctx.beginPath();
    // ctx.rect(x, y, w, h);
    ctx.fillStyle = color;
    ctx.fillRect(x, y, w, h);
}
