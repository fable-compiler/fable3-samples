const path = require('node:path');

const babelConf = {
  presets: [
    ["@babel/preset-env", {
      "modules":false,
      "corejs": 3,
      "useBuiltIns": "usage"
    }]
  ]
}

module.exports = {
  entry: './src/index.fs.js',
  output: {
    path: path.resolve(__dirname, 'build'),
    filename: 'index.js',
    libraryTarget: 'amd' // This is important
  },
  mode: "production",
  module: {
    rules: [
        {
            test: /\.js$/,
            include: path.resolve(__dirname, 'split'),
            exclude: /(node_modules|build)/,
            use: {
                loader: 'babel-loader',
                options: {
                    babel: babelConf
                }
            }
        }
    ]
  },
  externals: {
    'react': 'amd react' // this line is just to use the React dependency of our parent-testing-project instead of using our own React.
  }
};
