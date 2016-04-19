
app.factory("RegistrationsFactory", ["$resource", function ($resource) {

    return $resource('http://localhost:22903/api/registrations', {}, {
        query: { method: 'GET', isArray: true },
        create: { method: 'POST' }
    });

}]);

app.factory("RegistrationFactory", ["$resource", function ($resource) {

    return $resource('http://localhost:22903/api/registrations/:id', {}, {
        show: { method: 'GET' },
        update: { method: 'PUT', params: { id: '@id' } },
        delete: { method: 'DELETE', params: { id: '@id' } }
    });

}]);

app.factory("NotificationFactory", ["$resource",
    function ($resource) {
        return {
            notification: $resource('http://localhost:22903/api/notifications', {}, {
                notifyAll: { method: 'POST' }
            }),
            filters: $resource('http://localhost:22903/api/filters', {}, {
                get: { method: 'GET', isArray: true }
            })
        };
    }]);