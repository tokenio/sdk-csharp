#
# Fetches specified proto files from the artifact repository.
#
TOKEN_PROTOS_VER = "1.4.4"
RPC_PROTOS_VER = "1.1.48"


require 'open-uri'
require 'fileutils'

def fetch_protos()
    def download(path, name, type, version)
        file = "#{name}-#{type}-#{version}.jar"
        puts("Downloading #{file} ...")

        m2path = ENV["HOME"] + "/.m2/repository/#{path}/#{name}-#{type}/#{version}/#{file}"
        if File.file?(m2path) then
            FileUtils.cp(m2path, file)
        else
            url = "https://token.jfrog.io/token/libs-release/#{path}/#{name}-#{type}/#{version}/#{file}"
            open(file, 'wb') do |file|
                file << open(url).read
            end
        end
        file
    end

    system("rm protos/common/*.proto")
    system("rm -rf protos/external")

    file = download("io/token/proto", "tokenio-proto", "external", TOKEN_PROTOS_VER)
    puts("unzipping #{file}")
    system("unzip -d protos/external -o #{file} 'gateway/*.proto'")
    system("rm -f #{file}");

    file = download("io/token/proto", "tokenio-proto", "common", TOKEN_PROTOS_VER)
    puts("unzipping #{file}")
    system("unzip -d protos/common -o #{file} '*.proto'")
    system("unzip -d protos/common -o #{file} 'provider/*.proto'")
    system("unzip -d protos/common -o #{file} 'google/api/*.proto'")
    system("rm -f #{file}");

    file = download("io/token/rpc", "tokenio-rpc", "proto", RPC_PROTOS_VER)
    system("unzip -d protos -o #{file} '*.proto'")
    system("rm -f #{file}");
end

#
# Generates Objective-C code for the protos.
#
def generate_protos_cmd(path_to_protos, out_dir)
    # Base directory where the .proto files are.
    src = "./protos"

    # Pods directory corresponding to this app's Podfile, relative to the location of this podspec.
    pods_root = 'Pods'

    # Path where Cocoapods downloads protoc and the gRPC plugin.

    protoc_dir = "./tools/" + ((RUBY_PLATFORM.include?"linux") ? "linux_x64" : "macosx_x64")
    protoc = "#{protoc_dir}/protoc"
    plugin = "#{protoc_dir}/grpc_csharp_plugin"

    result = <<-CMD
       mkdir -p #{out_dir}
       #{protoc} \
           --plugin=protoc-gen-grpc=#{plugin} \
           --csharp_out=#{out_dir} \
           --grpc_out=#{out_dir} \
           -I #{src}/common \
           -I #{src}/external \
           -I #{src} \
           -I #{protoc_dir} \
           #{src}/#{path_to_protos}/*.proto
    CMD
    result
end


# Fetch the protos.
fetch_protos();

# Build the command that generates the protos.
core_dir = "./core/generated"
sdk_dir = "./sdk/generated"
system("rm -rf #{core_dir}");
system("rm -rf #{sdk_dir}");

#
gencommand = generate_protos_cmd("common", core_dir) +
generate_protos_cmd("common/provider", core_dir) +
generate_protos_cmd("common/google/api", core_dir) +
generate_protos_cmd("external/gateway", core_dir) +
generate_protos_cmd("extensions", core_dir)+
generate_protos_cmd("common", sdk_dir) +
generate_protos_cmd("common/provider", sdk_dir) +
generate_protos_cmd("common/google/api", sdk_dir) +
generate_protos_cmd("external/gateway", sdk_dir) +
generate_protos_cmd("extensions", sdk_dir);


system(gencommand)
