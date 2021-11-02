const DefinePlugin = require("webpack").DefinePlugin;

const HtmlWebpackPlugin = require("html-webpack-plugin");
const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin;
module.exports = (env, argv) => ({
    devServer: {
        publicPath: "auto",
        hot: true,
        headers: {
            "Cross-Origin-Opener-Policy": "same-origin",
            "Cross-Origin-Embedder-Policy": "require-corp",
        },
    },
    entry: "./src/App.fs.js",
    resolve: {
        extensions: [".dev.js", ".js", ".json", ".wasm", ".ts", ".tsx"],
        fallback: {
            crypto: false,
            path: false,
            fs: false,
            "react-native-sqlite-storage": false,
        },
    },
    module: {
    },
    plugins: [
        new HtmlWebpackPlugin({template: "./src/index.html"}),
        // new BundleAnalyzerPlugin({analyzerMode:"static"}),
        new DefinePlugin({
            "process.env.NODE_ENV": JSON.stringify(argv.mode),
            "process.env.PERF_BUILD": false,
        }),
    ],
    devtool: false,
});
