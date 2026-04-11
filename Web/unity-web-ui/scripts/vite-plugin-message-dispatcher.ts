import type { Plugin } from 'vite';
import fs from 'fs';
import path from 'path';

const regex = /export\s+class\s+(\w+)[\s\S]*?get\s+messageType\s*\(\s*\)[\s\S]*?return\s+([^;\s]+)\s*;?/g;

export interface MessageDispatcherPluginOptions {
    src: string;
    output: string;
    extensions?: string[];
}

export function VitePluginMessageDispatcher(options: MessageDispatcherPluginOptions): Plugin {
    const { src, output, extensions = ['ts'] } = options;
    return {
        name: 'vite-plugin-message-dispatcher',
        async buildStart() {
            try {
                const files = getAllFiles(src, extensions);
                const allInfos: MessageHandlerInfo[] = files.flatMap(file => buildInfo(fs.readFileSync(file, 'utf-8'), file));
                const pairs = groupAllInfo(allInfos);
                const content = buildCode(pairs, output);
                writeFile(output, content);
            } catch (error) {
                console.error('[MessageDispatcher] Error during scanning:', error);
            }
        }
    };
}

function getAllFiles(directory: string, extensions: string[], arrayOfFiles: string[] = []): string[] {
    if (!fs.existsSync(directory)) {
        console.warn(`[MessageDispatcher] Input directory does not exist: ${directory}`);
        return [];
    }

    const files = fs.readdirSync(directory);

    files.forEach(file => {
        const fullPath = path.join(directory, file);

        if (fs.statSync(fullPath).isDirectory()) {
            getAllFiles(fullPath, extensions, arrayOfFiles);
        } else {
            if (extensions.some(extension => file.endsWith(extension))) {
                arrayOfFiles.push(fullPath);
            }
        }
    });

    return arrayOfFiles;
}

interface MessageHandlerInfo {
    filePath: string;
    messageType: string;
    handlerType: string;
}

function buildInfo(content: string, filePath: string) {
    const infos: MessageHandlerInfo[] = [];
    let match;
    regex.lastIndex = 0;

    while ((match = regex.exec(content)) !== null) {
        infos.push({
            handlerType: match[1],
            messageType: match[2],
            filePath: filePath
        });
    }
    return infos;
}

function groupAllInfo(infos: MessageHandlerInfo[]): Record<string, MessageHandlerInfo[]> {
    const grouped: Record<string, MessageHandlerInfo[]> = {};
    for (const info of infos) {
        if (!grouped[info.messageType]) {
            grouped[info.messageType] = [];
        }
        grouped[info.messageType].push(info);
    }
    return grouped;
}

function buildCode(pairs: Record<string, MessageHandlerInfo[]>, output: string) {
    const importsByFile = new Map<string, Set<string>>();
    for (const infos of Object.values(pairs)) {
        for (const handler of infos) {
            if (!importsByFile.has(handler.filePath)) {
                importsByFile.set(handler.filePath, new Set());
            }
            importsByFile.get(handler.filePath)!.add(handler.handlerType);
        }
    }

    const importStatements: string[] = [
        'import { Opcode } from "@runtime/generated/message/Opcode";'
    ];

    for (const [filePath, classNames] of importsByFile) {
        const relativePath = path.relative(path.dirname(output), filePath)
            .replace(/\\/g, '/')
            .replace(/\.tsx?$/, '');
        const importPath = relativePath.startsWith('.') ? relativePath : `./${relativePath}`;
        importStatements.push(`import { ${Array.from(classNames).join(', ')} } from "${importPath}";`);
    }

    importStatements.push('import type { IWebMessageHandler } from "./IMessage";');

    const mappingLines: string[] = [];
    for (const [messageType, handlers] of Object.entries(pairs)) {
        mappingLines.push(`\t\tthis.allHandler.set(Opcode.${messageType}, [${handlers.map(handler => `new ${handler.handlerType}()`).join(', ')}]);`);
    }

    return `${importStatements.join('\n')}

class UnityMessageDispatcherType {
    private readonly allHandler = new Map<number, IWebMessageHandler[]>();

    constructor() {
${mappingLines.join('\n')}
    }

    handle(opcode: number, message: Object) {
        const handlers = this.allHandler.get(opcode);
        if (handlers === undefined) {
            console.warn(\`消息 \${opcode} 无处理器\`);
            return;
        }

        for (const handler of handlers) {
            try {
                handler.handle(message);
            } catch (error) {
                console.error(\`消息 \${opcode} 处理失败\`, error);
            }
        }
    }
}

export const UnityMessageDispatcher = new UnityMessageDispatcherType();
`;
}

function writeFile(output: string, content: string): void {
    const directory = path.dirname(output);

    if (!fs.existsSync(directory)) {
        fs.mkdirSync(directory, { recursive: true });
    }

    if (fs.existsSync(output)) {
        const existing = fs.readFileSync(output, 'utf-8');
        if (existing === content) {
            return;
        }
    }

    fs.writeFileSync(output, content, 'utf-8');
    console.log(`[MessageDispatcher] ✓ Generated.`);
}