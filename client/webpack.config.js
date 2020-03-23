const path = require('path');

module.exports = {
    // entry: './src/main.ts',
    // mode: "production",
    entry: './src/index.ts',
    // Enable sourcemaps for debugging webpack's output.
    // devtool: "source-map",
    output: {
        path: path.resolve(__dirname, 'dist'),
        filename: 'index.js',
    },
    resolve: {
        extensions: ['.ts', '.tsx', '.js', '.jsx'],
    },
    module: {
        rules: [
            {
                test: /\.ts(x?)$/,
                exclude: /node_modules/,
                use: [
                    {
                        loader: "ts-loader"
                    }
                ]
            },
            // All output '.js' files will have any sourcemaps re-processed by 'source-map-loader'.
            {
                enforce: "pre",
                test: /\.js$/,
                loader: "source-map-loader"
            }
        ]
    },
    devServer: {
        contentBase: path.join(__dirname, 'dist'),
        // proxy: {
        //     '/snakeHub': {
        //         target: 'http://0.0.0.0:5000',
        //         changeOrigin: true,
        //         logLevel: "debug"
        //         // secure: false,
        //         // pathRewrite: {'^/snakeHub' : ''}
        //     }
        // },
        port: 8080
    },
};
