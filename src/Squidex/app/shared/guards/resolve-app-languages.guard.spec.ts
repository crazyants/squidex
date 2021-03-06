/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { IMock, Mock } from 'typemoq';
import { Observable } from 'rxjs';

import { AppLanguageDto, AppLanguagesService } from 'shared';

import { ResolveAppLanguagesGuard } from './resolve-app-languages.guard';
import { RouterMockup } from './router-mockup';

describe('ResolveAppLanguagesGuard', () => {
    const route = {
        params: {
            appName: 'my-app'
        }
    };

    let appLanguagesService: IMock<AppLanguagesService>;

    beforeEach(() => {
        appLanguagesService = Mock.ofType(AppLanguagesService);
    });

    it('should throw if route does not contain parameter', () => {
        const guard = new ResolveAppLanguagesGuard(appLanguagesService.object, <any>new RouterMockup());

        expect(() => guard.resolve(<any>{ params: {} }, <any>{})).toThrow('Route must contain app name.');
    });

    it('should navigate to 404 page if languages are not found', (done) => {
        appLanguagesService.setup(x => x.getLanguages('my-app', null))
            .returns(() => Observable.of(null!));
        const router = new RouterMockup();

        const guard = new ResolveAppLanguagesGuard(appLanguagesService.object, <any>router);

        guard.resolve(<any>route, <any>{})
            .then(result => {
                expect(result).toBeFalsy();
                expect(router.lastNavigation).toEqual(['/404']);

                done();
            });
    });

    it('should navigate to 404 page if languages loading fails', (done) => {
        appLanguagesService.setup(x => x.getLanguages('my-app', null))
            .returns(() => Observable.throw(null!));
        const router = new RouterMockup();

        const guard = new ResolveAppLanguagesGuard(appLanguagesService.object, <any>router);

        guard.resolve(<any>route, <any>{})
            .then(result => {
                expect(result).toBeFalsy();
                expect(router.lastNavigation).toEqual(['/404']);

                done();
            });
    });

    it('should return schema if loading succeeded', (done) => {
        const languages: AppLanguageDto[] = [];

        appLanguagesService.setup(x => x.getLanguages('my-app', null))
            .returns(() => Observable.of(languages));
        const router = new RouterMockup();

        const guard = new ResolveAppLanguagesGuard(appLanguagesService.object, <any>router);

        guard.resolve(<any>route, <any>{})
            .then(result => {
                expect(result).toBe(languages);

                done();
            });
    });
});