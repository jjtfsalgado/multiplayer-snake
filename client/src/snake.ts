export const canvas = document.getElementById("canvas") as HTMLCanvasElement;
canvas.height = 500;
canvas.width = 600;
export const ctx = canvas.getContext('2d');

function Trail(x: number, y: number) {
    this.x = x;
    this.y = y;
}

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

    constructor(props: {id: string, description: string, x?: number, y?: number, direction?, speed?, trail?, tail?}) {
        const {description,id, direction, speed, tail, trail, x, y} = props;
        this.id = id;
        this.x = x || canvas.width / 2;
        this.y = y || canvas.height / 2;
        this.description = description;
        this.direction = "xx" || direction;
        this.speed = 1 || speed;
        this.trail = [] || trail;
        this.tail = 50 || tail;
        document.addEventListener("keydown", this.onKeyDown);
    }

    private onKeyDown = (ev: any) => {
        switch (ev.keyCode) {
            case 37: {
                //left arrow
                this.changeDirection("-xx");
                break
            }
            case 38: {
                //up arrow
                this.changeDirection("yy");
                break
            }
            case 39: {
                //right arrow
                this.changeDirection("xx");
                break;
            }
            case 40: {
                //down arrow
                this.changeDirection("-yy");
                break;
            }
        }
    };

    changeDirection = (type: "xx" | "yy" | "-yy" | "-xx") => {
        if (this.direction !== type) {
            this.direction = type;
        }
    };

    addTrail = () => {
        this.trail.push(new Trail(this.x, this.y));
        if (this.trail.length >= this.tail) {
            this.trail.shift();
        }
    };

    move = () => {
        switch (this.direction) {
            case "xx": {
                this.x += this.speed;
                break;
            }
            case "-xx": {
                this.x -= this.speed;
                break;
            }
            case "yy": {
                this.y -= this.speed;
                break;
            }
            case "-yy": {
                this.y += this.speed;
                break;
            }
        }

        if (this.x === canvas.width) {
            this.x = 0;
        } else if (this.y === canvas.height) {
            this.y = 0;
        } else if(this.x === 0){
            this.x = canvas.width;
        } else if(this.y === 0){
            this.y = canvas.height;
        }
    };

    render = (isMain?) => {
        this.move();
        this.addTrail();

        this.trail.forEach(i => {
            ctx.beginPath();
            ctx.rect(i.x, i.y, 5, 5);
            ctx.fillStyle = isMain ? 'red' : 'green';
            ctx.fill();
        });
    };
}


