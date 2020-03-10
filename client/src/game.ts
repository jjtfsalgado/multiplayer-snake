import * as signalR from "@microsoft/signalr";
import {HubConnection, HubConnectionState} from "@microsoft/signalr";
import {canvas, ctx, renderPixel, Snake} from "./snake";

function randomPixel(limit: number) {
    return Math.floor(Math.random() * limit) + 1
}

function Food() {
    this.x = randomPixel(canvas.width);
    this.y = randomPixel(canvas.height);
}

let name = localStorage.getItem("snakeName");

if(!name){
    name = prompt("Hey!!!! Name pleaseee?");
    localStorage.setItem("snakeName", name);
}

const board = document.getElementById("board");

export class Game {
    food: Array<{x: number, y: number}> = [];
    snakes: Array<Snake> = [];
    snake: Snake;
    connection: HubConnection;

    constructor() {
        this.connection = new signalR.HubConnectionBuilder().withUrl("http://192.168.5.45:5000/snakeHub").build();
        this.connection.on("Snakes", this.onSnakes);
        window.addEventListener("close", this.onWindowClose);
        window.addEventListener("keydown", this.onKeyDown);
    }

    private onWindowClose = async () => {
        await this.connection.stop();
    };

    addSnake = (): Snake => {
        return new Snake({id: this.connection.connectionId, description: name}, this.connection)
    };

    start = async () => {
        this.renderFrame();
        try{
            await this.connection.start();
            this.snake = this.addSnake();
            this.snakes.push(this.snake);

            await this.connection.invoke("connectedSnake", JSON.stringify(this.snake), JSON.stringify({width: canvas.width, height: canvas.height}));
            console.info("connected");
        } catch (e) {
            console.error(e.toString());
        }
    };

    changeDirection = async (type: "xx" | "yy" | "-yy" | "-xx") => {
        if (this.snake.direction !== type) {
            this.snake.direction = type;
        }

        await this.connection.invoke("changeDirection", JSON.stringify(this.snake));
    };

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


    // addFood = (shift? : boolean) => {
    //     this.food = this.food || [];
    //     if(shift){
    //         this.food.shift()
    //     }
    //
    //     this.food.push(new Food());
    // };

    onSnakes = (snakes, food) => {
        const s: Array<any> = JSON.parse(snakes);
        this.food = food && JSON.parse(food) || [];

        const copy = [...s];

        sortBy(copy, i => i.tail, {desc: true});
        board.innerHTML = "";

        copy.forEach(i => {
            const li = document.createElement('li');
            li.setAttribute('class','item');
            li.innerHTML = i.description + " : " + i.tail;
            board.appendChild(li);
        });


        // console.log("hey i just received some snakes -> ", s, food);

        this.snakes = s.map(i => {
            const existing = this.snakes.find(j => j.id === i.id);

            if (existing) {
                Object.assign(existing, i);
                return existing;
            } else {
                return new Snake(i, this.connection);
            }
        });

        this.renderFrame();
    };

    renderFrame = () => {
        //todo in case of error stop the rendering
        // if(this.connection.state as HubConnectionState === "Disconnected"){
        //     return
        // }

        ctx.clearRect(0 ,0 , canvas.width, canvas.height);

        this.snakes.forEach(i => {
            i.render(i.id === this.connection.connectionId);
        });

        this.food.forEach(i => renderPixel(i.x, i.y, 5, 5, "white"));
        // requestAnimationFrame(this.renderFrame)
    };
}

const game = new Game();

(async () => {
    await game.start()
})();

export function sortBy<T>(arr: Array<T>, by: (i: T) => any, opts?: { desc?: boolean }): void {
    const desc = opts && opts.desc;
    arr.sort((a, b) => {
        const va = undefault(by(a), null);
        const vb = undefault(by(b), null);
        return desc
            ? ( (va < vb) ? 1 : (va > vb) ? -1 : 0 )
            : ( (va < vb) ? -1 : (va > vb) ? 1 : 0 )
            ;
    });
}

export function undefault(val, def) {
    return val === undefined ? def : val;
}
