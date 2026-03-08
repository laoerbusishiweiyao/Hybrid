import { reactive, ref } from "vue";
import { createRouter, createWebHistory, type Router, type RouteRecordRaw } from "vue-router";

export class RouteMeta {
    Title?: string;
    Icon?: string;
    Priority?: number;
    MenuVisibility?: boolean;
}

export class RouteConfig {
    public Path!: string;
    public Name!: string;
    public Components?: Array<Array<string>>;
    public Meta: RouteMeta = {};
    public Children?: Array<RouteConfig>;
}

const dynamicRouteConfigs: Array<RouteConfig> = [];

export class MenuItemConfig {
    Id!: number;
    Title!: string;
    Index!: string;
    Icon?: string;
    Children?: MenuItemConfig[];
    Visibility!: boolean;
}

export class BreadcrumbItemConfig {
    Icon!: string;
    Title!: string;
    Path!: string;
}

export class MenuTabConfig {
    Title!: string;
    Icon!: string;
    Closeable!: boolean;
    Path!: string;
}

class RouterContextType {
    public static SessionStorageKey: string = 'Router';
    private static MenuItemIdCounter: number = 0;

    public Router: Router = createRouter(
        {
            history: createWebHistory(),
            routes: [],
        }
    );

    public DefaultActiveMenu: string = '/analytics';

    public CurrentMenuTab = ref(this.DefaultActiveMenu);

    /** 菜单列表 */
    public MenuItemConfigs: Array<MenuItemConfig> = [];

    /** 面包屑列表 */
    public BreadcrumbItemConfigs: Array<BreadcrumbItemConfig> = reactive([]);

    /** 菜单标签 */
    public MenuTabConfigs: Array<MenuTabConfig> = reactive([]);

    private pages: Record<string, () => Promise<unknown>> = {};

    constructor() {
        const home = {
            path: '/', name: '', components: {
                default: () => import('@pages/HomePage.vue'),
            }
        };
        this.Router.addRoute(home);

        for (const [path, promise] of Object.entries(import.meta.glob('@pages/*Page.vue'))) {
            const key = path.replace(/^\/src\/pages\//, '').replace(/\.vue$/, '');
            this.pages[key] = promise;
        }

        this.addDynamicRoutes(dynamicRouteConfigs);

        this.MenuItemConfigs.length = 0;
        this.MenuItemConfigs.push(...this.generateMenuItems(dynamicRouteConfigs[1]?.Children ?? []));
    }

    addDynamicRoutes(configs: Array<RouteConfig>, parent: string = ''): void {
        for (const config of configs) {
            this.Router.addRoute(this.generateRouteRecordRaw(parent, config));
        }
    }

    private generateMenuItems(configs: RouteConfig[], parentPath: string = ''): MenuItemConfig[] {
        const menuItems: MenuItemConfig[] = [];
        for (const config of configs) {
            if (config.Meta.MenuVisibility !== true) {
                continue;
            }

            const fullPath = parentPath ? `${parentPath}/${config.Path}`.replace(/\/+/g, '/') : `/${config.Path}`.replace(/\/+/g, '/')
            const menuItem: MenuItemConfig = {
                Id: RouterContextType.MenuItemIdCounter++,
                Title: config.Meta.Title || config.Name,
                Index: fullPath,
                Icon: config.Meta?.Icon || 'Menu',
                Visibility: config.Meta.MenuVisibility,
            }

            if (config.Children && config.Children.length > 0) {
                const children = this.generateMenuItems(config.Children, fullPath)
                if (children.length > 0) {
                    menuItem.Children = children
                }
            }

            menuItems.push(menuItem)
        }

        return menuItems;
    }

    public updateCurrentMenu(path: string): void {
        this.BreadcrumbItemConfigs.splice(0, this.BreadcrumbItemConfigs.length);

        const normalizedPath = path.replace(/\/+$/, '').replace(/^\/?(.*)/, '/$1');
        const segments = normalizedPath.split('/').filter(Boolean);

        if (segments.length === 0) {
            this.BreadcrumbItemConfigs.splice(0, this.BreadcrumbItemConfigs.length, ...[{ Icon: 'Home', Title: '首页', Path: '/home' }]);
            return;
        }

        let configs = dynamicRouteConfigs[1]?.Children ?? [];
        let currentPath = '';

        for (const segment of segments) {
            const config = configs.find(item => item.Path == segment);
            if (!config) {
                throw new Error(`Path ${segment} not found`);
            }

            currentPath = currentPath ? `${currentPath}/${segment}` : `/${segment}`;

            this.BreadcrumbItemConfigs.push({
                Icon: config.Meta?.Icon || 'Menu',
                Title: config.Meta?.Title || config.Name || segment,
                Path: currentPath
            });

            configs = config.Children ?? [];
        }

        this.CurrentMenuTab.value = path;

        if (this.MenuTabConfigs.findIndex(item => item.Path == path) > -1) {
            return;
        }

        const config = this.BreadcrumbItemConfigs[this.BreadcrumbItemConfigs.length - 1];
        if (!config) {
            throw new Error('Breadcrumb item not found');
        }
        this.MenuTabConfigs.push({
            Title: config.Title,
            Icon: config.Icon,
            Path: config.Path,
            Closeable: true
        });
    }

    private generateRouteRecordRaw(parent: string, config: RouteConfig): RouteRecordRaw {
        const path = `${parent}/${config.Path}`;

        const components: Record<string, () => Promise<unknown>> = {};
        if (config.Components) {
            for (const [name, key] of config.Components) {
                if (!name || !key) {
                    continue;
                }
                const page = this.pages[key];
                if (!page) {
                    console.error(`Page ${name} not found`);
                    continue;
                }
                components[name] = page;
            }
        }

        const children: Array<RouteRecordRaw> = [];
        if (config.Children != undefined) {
            for (const child of config.Children) {
                children.push(this.generateRouteRecordRaw(path, child));
            }
        }

        return {
            name: config.Name,
            path,
            components,
            children,
        };
    }
}

export const RouterContext = new RouterContextType();