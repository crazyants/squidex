﻿      var webpack = require('webpack'),
     webpackMerge = require('webpack-merge'),
ExtractTextPlugin = require('extract-text-webpack-plugin'),
        runConfig = require('./webpack.run.base.js'),
          helpers = require('./helpers');

const ENV = process.env.NODE_ENV = process.env.ENV = 'production';

commonConfig.plugins[0].options.options.tslint.emitErrors = true;

module.exports = webpackMerge(runConfig, {
    devtool: 'source-map',

    output: {
        /**
         * The output directory as absolute path (required).
         *
         * See: http://webpack.github.io/docs/configuration.html#output-path
         */
        path: helpers.root('wwwroot/build/'),

        publicPath: '/build/',

        /**
         * Specifies the name of each output file on disk.
         * IMPORTANT: You must not specify an absolute path here!
         *
         * See: http://webpack.github.io/docs/configuration.html#output-filename
         */
        filename: '[name].[hash].js',

        /**
         * The filename of non-entry chunks as relative path
         * inside the output.path directory.
         *
         * See: http://webpack.github.io/docs/configuration.html#output-chunkfilename
         */
        chunkFilename: '[id].[hash].chunk.js'
    },

    /*
     * Options affecting the normal modules.
     *
     * See: http://webpack.github.io/docs/configuration.html#module
     */
    module: {
        /**
         * An array of automatically applied loaders.
         *
         * IMPORTANT: The loaders here are resolved relative to the resource which they are applied to.
         * This means they are not resolved relative to the configuration file.
         *
         * See: http://webpack.github.io/docs/configuration.html#module-loaders
         */
        loaders: [
            {
                test: /\.scss$/,
                include: helpers.root('app', 'theme'),
                loader: ExtractTextPlugin.extract({ fallbackLoader: 'style', loader: 'css!sass?sourceMap' })
            }
        ]
    },

    plugins: [
        new webpack.NoErrorsPlugin(),
        new webpack.DefinePlugin({ 'process.env': { 'ENV': JSON.stringify(ENV) } }),

        new webpack.optimize.UglifyJsPlugin({ 
            mangle: { 
                screw_ie8: true, keep_fnames: true 
            } 
        }),

        function () {
            this.plugin('done', function (stats) {
                if (stats.compilation.errors && stats.compilation.errors.length && process.argv.indexOf('--watch') == -1) {
                    console.log(stats.compilation.errors);

                    throw new Error('webpack build failed.');
                }
            });
        }
    ]
});