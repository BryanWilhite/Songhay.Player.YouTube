import { dirname, resolve as _resolve } from 'path';
import { fileURLToPath } from 'url';

import TerserJSPlugin from 'terser-webpack-plugin';
import pkg from 'webpack';
const { ProgressPlugin } = pkg;

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

const sharedConfig = {
    entry: {
        scripts: [
            './src/ts/_index.ts',
        ]
    },
    plugins: [
        new ProgressPlugin(),
    ],
    module: {
        rules: [
            {
                test: /\.tsx?$/,
                use: [
                    {
                        loader: 'ts-loader',
                        options: { configFile: 'tsconfig.json'}
                    }
                ],
            },
        ],
    },
    resolve: {
        extensions: ['.tsx', '.ts', '.js'],
    },
};

const outputLibraryConfig = {
    library: {
        name: 'rx',
        type: 'var',
    },
};

const defaultConfig = {
    name: 'default-config',
    output: {
        filename: 'songhay-player-yt.js',
        path: _resolve(__dirname, 'wwwroot', 'js'),
        ...outputLibraryConfig
    },
    optimization: {
        minimize: false,
    },
};

const optimizationConfig = {
    name: 'optimization-config',
    output: {
        filename: 'songhay-player-yt.min.js',
        path: _resolve(__dirname, 'wwwroot', 'js'),
        ...outputLibraryConfig
    },
    optimization: {
        minimizer: [new TerserJSPlugin({})],
    },
};

export default [
    {...sharedConfig,...defaultConfig},
    {...sharedConfig,...optimizationConfig},
];
